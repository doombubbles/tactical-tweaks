using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Behaviors;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Behaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using Il2CppSystem.IO;
using Newtonsoft.Json;
using Vector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using Vector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace TacticalTweaks.Tweaks;

public class PursuitPathPrioritization : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    protected override string Icon => VanillaSprites.PursuitUpgradeIcon;

    public override string Description => "Allows Heli Pilots to choose to prioritize Bloons on specific paths.";

    public static readonly Dictionary<ObjectId, List<string>> PathSelections = new();

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.HeliPilot).ToArray())
        {
            towerModel.towerSelectionMenuThemeId = GetId<PursuitTsmTheme>();
        }
    }

    public override void OnTowerSaved(Tower tower, TowerSaveDataModel saveData)
    {
        if (PathSelections.TryGetValue(tower.Id, out var paths))
        {
            saveData.metaData[Id] = JsonConvert.SerializeObject(paths);
        }
    }

    public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
    {
        if (saveData.metaData.TryGetValue(Id, out var pathJson))
        {
            PathSelections[tower.Id] = JsonConvert.DeserializeObject<List<string>>(pathJson) ?? [];
        }
    }

    public override void OnGameObjectsReset()
    {
        PathSelections.Clear();
    }

    [HarmonyPatch(typeof(PursuitSetting), nameof(PursuitSetting.GetPursuitTarget))]
    internal static class PursuitSetting_GetPursuitTarget
    {
        [HarmonyPrefix]
        internal static bool Prefix(PursuitSetting __instance, ref Target? __result)
        {
            if (__instance.heliMovement?.tower == null ||
                !PathSelections.TryGetValue(__instance.heliMovement.tower.Id, out var paths)) return true;

            var currentPaths = paths
                .Select(p => __instance.Sim.Map.pathManager.GetPath(p))
                .Where(p => p.hasBloons || p.hasOfftrackBloons)
                .ToArray();

            if (!currentPaths.Any()) return true;

            __result = __instance.Targets(99999).Cast<Il2CppSystem.Collections.Generic.IEnumerable<Target>>().ToArray()
                .FirstOrDefault(target =>
                {
                    var path = (target.pathSegment?.path ?? target.bloon?.path);

                    return path != null && (currentPaths.Contains(path) || path.def.pathId.Contains("Boss"));

                });

            __result ??= new Target();

            return false;
        }
    }

    public class PursuitTsmTheme : ModTsmTheme
    {
        public override string BaseTheme => "Default";

        private TSMButton button = null!;

        public override void SetupTheme(BaseTSMTheme theme)
        {
            button = theme.gameObject.AddTSMButton(new("PursuitButton", RightArrowX, AboveArrowsY, DefaultButtonSize),
                VanillaSprites.GreenBtnSquare, "PursuitButton", GetId<PursuitCustomInput>());

            button.gameObject.AddImage(new("Icon", DefaultIconSize), VanillaSprites.PursuitUpgradeIcon);
        }

        public override void TowerChanged(BaseTSMTheme theme, TowerToSimulation tower)
        {
            var pursuit = tower.tower.Entity.GetBehaviorInDependants<PursuitSetting>();

            if (pursuit == null)
            {
                button.gameObject.SetActive(false);
                return;
            }

            button.gameObject.SetActive(true);

            if (!Dots.Any())
            {
                StartDrawing();
            }
        }

        #region Drawing Path Dots

        private static readonly Dictionary<Entity, string> Dots = [];

        public override void OnDisable(BaseTSMTheme theme)
        {
            StopDrawing();
        }

        private static void StartDrawing()
        {
            var sim = Simulation.Current;

            foreach (var path in sim.Map.pathManager.paths)
            {
                if (!path.isActive || path.def.pathId.Contains("Boss")) continue;
                foreach (var pathSegment in path.segments)
                {
                    var entity = sim.factory.Create<Entity>();
                    entity.AddBehaviorFromModel(TransformModel.Create(new()
                    {
                        position = new Vector3(pathSegment.point.x, pathSegment.point.y, pathSegment.pointHeight),
                    }));
                    var display = entity.AddBehaviorFromModel(DisplayModel.Create()).Cast<DisplayBehavior>();
                    display.SetScaleOffset(Vector3.zero);

                    Dots[entity] = path.def.pathId;
                }
            }
        }

        private static void StopDrawing()
        {
            foreach (var entity in Dots.Keys)
            {
                entity.Destroy();
            }
            Dots.Clear();
        }

        public override void Update(BaseTSMTheme theme)
        {
            const string patrolPointLineMarkerYellow = "505654c2ff7054e49900a08b16f67bde";
            const string patrolPointLineMarkerGreen = "f6e4526492bc9164baac0dd1058077f2";
            const string patrolPointLineMarkerRed = "15a96ce311133ec46b824e88f291a255";

            if (!Dots.Any()) return;

            var inputManager = InGame.instance.InputManager;
            var inInput = ModCustomInput.ActiveInput is PursuitCustomInput;

            var tower = inInput ? inputManager.customInput?.tower : TowerSelectionMenu.instance.selectedTower;

            var currentHoveredPath = inInput
                ? PursuitCustomInput.GetPath(inputManager.CursorPositionWorld)?.path?.def.pathId
                : null;

            var pathIds = tower == null ? [] : PathSelections.GetValueOrDefault(tower.Id, []);

            foreach (var (entity, pathId) in Dots)
            {
                var display = entity.GetDisplayBehavior();

                display.SetScaleOffset(Vector3.one *
                                       (tower != null && (pathIds.Contains(pathId) || inInput) ? 1 : 0));

                var currentDisplay = display.displayModel.display?.AssetGUID;
                var newDisplay = patrolPointLineMarkerGreen;
                if (inInput)
                {
                    var isHovered = currentHoveredPath == pathId;
                    var isSelected = pathIds.Contains(pathId);

                    if (isHovered && isSelected) newDisplay = patrolPointLineMarkerRed;
                    else if (!isHovered && !isSelected) newDisplay = patrolPointLineMarkerYellow;
                }

                if (currentDisplay != newDisplay)
                {
                    display.displayModel.display = new PrefabReference(newDisplay);
                    display.UpdatedModel(display.displayModel);
                }
            }
        }

        #endregion

    }

    public class PursuitCustomInput : ModCustomInput
    {
        public override string GetHelperMessage(CustomInput customInput) => "Prioritize pursuing specific path(s)";

        public static PathSegment? GetPath(UnityEngine.Vector2 cursorPosWorld) =>
            Simulation.Current.map.pathManager.FirstPathSegmentInRangeOrDefault(new Vector2(cursorPosWorld), 10);

        public override bool IsPositionValid(CustomInput customInput, UnityEngine.Vector2 cursorPosWorld,
            bool isCursorInWorld)
        {
            return isCursorInWorld && GetPath(cursorPosWorld) != null;
        }

        public override void OnValidPositionCursorUp(CustomInput customInput, UnityEngine.Vector2 cursorPosWorld,
            bool isCursorInWorld)
        {
            var path = GetPath(cursorPosWorld)!.path.def.pathId;

            if (!PathSelections.TryGetValue(customInput.tower.Id, out var paths))
            {
                paths = [];
                PathSelections.Add(customInput.tower.Id, paths);
            }

            if (paths.Contains(path))
            {
                paths.Remove(path);
            }
            else
            {
                paths.Add(path);
            }

            base.OnValidPositionCursorUp(customInput, cursorPosWorld, isCursorInWorld);
        }

        public override void Update(CustomInput customInput, UnityEngine.Vector3 cursorPosUnityWorld,
            UnityEngine.Vector2 cursorPosWorld, bool isCursorActive)
        {
            if (!isCursorActive) return;

            var path = GetPath(cursorPosWorld)?.path.def.pathId;

            var message = GetHelperMessage(customInput);

            var pathIds = PathSelections.GetValueOrDefault(customInput.tower.Id, []);

            if (path != null)
            {
                message = pathIds.Contains(path) ? $"Stop prioritizing {path}" : $"Start prioritizing {path}";
            }

            customInput.inputManager.SetHelperMessage(message);
        }
    }
}