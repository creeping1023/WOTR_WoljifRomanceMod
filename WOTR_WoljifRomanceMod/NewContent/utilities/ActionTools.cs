﻿using TabletopTweaks;
using TabletopTweaks.Config;
using TabletopTweaks.Utilities;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Kingmaker.Utility;
using JetBrains.Annotations;
using System;


namespace WOTR_WoljifRomanceMod
{
    public class ControlWeather : Kingmaker.ElementsSystem.GameAction
    {
        public Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType inclemency;
        public bool start;
        public Kingmaker.Controllers.WeatherController control;
        public override string GetCaption()
        {
            return "Overrides weather conditions.";
        }

        public override void RunAction()
        {
            control = Kingmaker.Controllers.WeatherController.Instance;
            if (start)
            {
                control.StartOverrideInclemency(inclemency);
            }
            else
            {
                control.StopOverrideInclemency();
            }
        }
    }

    // Oh my god I can't believe this actually works. Stupid problems require stupid solutions.
    public class SetFlyHeight : Kingmaker.ElementsSystem.GameAction
    {
        public Kingmaker.ElementsSystem.UnitEvaluator Unit;
        public float height;
        public override string GetCaption()
        {
            return "Sets FlyHeight to make units hover above the ground.";
        }

        public override void RunAction()
        {
            Unit.GetValue().FlyHeight = height;
        }
    }

    // Here be dragons; completely untested.
    public class SpawnUnit : Kingmaker.ElementsSystem.GameAction
    {
        public Kingmaker.Blueprints.BlueprintUnit unit;
        public FakeLocator location;
        public Kingmaker.EntitySystem.Entities.UnitEntityData entity;
        public override string GetCaption()
        {
            return "UNTESTED: Spawn a unit.";
        }

        public override void RunAction()
        {
            entity = Game.Instance.EntityCreator.SpawnUnit(unit, location.GetValue(), UnityEngine.Quaternion.identity, null);
        }

        public Kingmaker.EntitySystem.Entities.UnitEntityData GetEntity()
        {
            return entity;
        }
    }


    public static class ActionTools
    {
        public static T GenericAction<T>(Action<T> init = null) where T : Kingmaker.ElementsSystem.GameAction, new()
        {
            var result = (T)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            init?.Invoke(result);
            return result;
        }

        /*public static SetTimer SetTimerAction(string timername)
        {
            var result = GenericAction<SetTimer>( bp => { bp.timername = timername; });
            return result;
        }*/

        public static Kingmaker.ElementsSystem.ActionList MakeList(Kingmaker.ElementsSystem.GameAction action)
        {
            var result = new Kingmaker.ElementsSystem.ActionList();
            Array.Resize(ref result.Actions, 1);
            result.Actions[0] = action;
            return result;
        }
        public static Kingmaker.ElementsSystem.ActionList MakeList(params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var result = new Kingmaker.ElementsSystem.ActionList();
            var len = actions.Length;
            Array.Resize(ref result.Actions, len);
            for (int i = 0; i < len; i++)
            {
                result.Actions[i] = actions[i];
            }
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight WeightedAction(Kingmaker.ElementsSystem.GameAction action, int weight, Kingmaker.ElementsSystem.Condition condition = null)
        {
            Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight result;
            result.Conditions = new Kingmaker.ElementsSystem.ConditionsChecker();
            if (condition != null)
            {
                ConditionalTools.CheckerAddCondition(result.Conditions, condition);
            }
            result.Action = MakeList(action);
            result.Weight = weight;
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction RandomAction(params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var len = actions.Length;
            Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[] WeightedActions = new Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[len];
            for (int i = 0; i < len; i++)
            {
                WeightedActions[i] = WeightedAction(actions[i], 1);
            }
            return RandomAction(WeightedActions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction RandomAction(params Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[] WeightedActions)
        {
            var len = WeightedActions.Length;
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction>(bp =>
            {
                bp.Actions = new Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[len];
                for (int i = 0; i < len; i++)
                {
                    bp.Actions[i] = WeightedActions[i];
                }
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.ShowBark BarkAction(string name, Companions target = Companions.None)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.ShowBark>(bp =>
            {
                bp.TargetUnit = CommandTools.getCompanionEvaluator(target);
                bp.BarkDurationByText = true;
                bp.WhatToBarkShared = new Kingmaker.Localization.SharedStringAsset();
                bp.WhatToBarkShared.String = new Kingmaker.Localization.LocalizedString { m_Key = name };
                bp.WhatToBark = bp.WhatToBarkShared.String;
            });
            return result;
        }

        public static SetFlyHeight SetFlyHeightAction(Companions unit, float height)
        {
            var result = GenericAction<SetFlyHeight>(bp =>
            {
                bp.Unit = CommandTools.getCompanionEvaluator(unit);
                bp.height = height;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.HideWeapons HideWeaponsAction(Companions unit, bool hide = true)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideWeapons>(bp =>
            {
                bp.Target = CommandTools.getCompanionEvaluator(unit);
                bp.Hide = hide;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.LockFlag LockFlagAction(Kingmaker.Blueprints.BlueprintUnlockableFlag flag)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.LockFlag>(bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
            });
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag UnlockFlagAction(Kingmaker.Blueprints.BlueprintUnlockableFlag flag)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag>(bp =>
            {
                bp.m_flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
            });
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue IncrementFlagAction(Kingmaker.Blueprints.BlueprintUnlockableFlag flag, int amount = 1)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>(bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
                bp.Value = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = amount };
                bp.UnlockIfNot = true;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty TeleportAction(string exitposition, Kingmaker.ElementsSystem.ActionList afterTeleport = null)
        {
            var result = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty>(bp =>
            {
                bp.m_exitPositon = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAreaEnterPointReference>(Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintAreaEnterPoint>(exitposition));
                if (afterTeleport == null)
                {
                    bp.AfterTeleport = DialogTools.EmptyActionList;
                }
                else
                {
                    bp.AfterTeleport = afterTeleport;
                    bp.AutoSaveMode = Kingmaker.EntitySystem.Persistence.AutoSaveMode.None;
                }
            });
            return result;
        }

        public static ControlWeather EndWeatherAction()
        {
            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = false;
                bp.inclemency = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            });
            return action;
        }

        public static ControlWeather SetWeatherAction(string intensity)
        {
            Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            switch (intensity)
            {
                case "Clear":
                case "clear":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
                    break;
                case "Light":
                case "light":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Light;
                    break;
                case "Moderate":
                case "moderate":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Moderate;
                    break;
                case "Heavy":
                case "heavy":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Heavy;
                    break;
                case "Storm":
                case "storm":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Storm;
                    break;
            }

            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = true;
                bp.inclemency = level;
            });
            return action;
        }
        public static ControlWeather SetWeatherAction(int intensity)
        {
            Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            switch (intensity)
            {
                case 0:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
                    break;
                case 1:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Light;
                    break;
                case 2:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Moderate;
                    break;
                case 3:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Heavy;
                    break;
                case 4:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Storm;
                    break;
            }

            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = true;
                bp.inclemency = level;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip SkipTimeAction(int minutes, bool nofatigue = true)
        {
            var skip = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = minutes };
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip>(bp =>
            {
                bp.NoFatigue = nofatigue;
                bp.m_Type = Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip.SkipType.Minutes;
                bp.MinutesToSkip = skip;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip SkipToTimeAction(string timeofday, bool nofatigue = true)
        {
            var time = Kingmaker.AreaLogic.TimeOfDay.Morning;
            switch (timeofday)
            {
                case "Morning":
                case "morning":
                    time = Kingmaker.AreaLogic.TimeOfDay.Morning;
                    break;
                case "Day":
                case "day":
                case "daytime":
                case "Daytime":
                case "afternoon":
                case "Afternoon":
                    time = Kingmaker.AreaLogic.TimeOfDay.Day;
                    break;
                case "Evening":
                case "evening":
                    time = Kingmaker.AreaLogic.TimeOfDay.Evening;
                    break;
                case "Night":
                case "night":
                case "Nighttime":
                case "nighttime":
                    time = Kingmaker.AreaLogic.TimeOfDay.Night;
                    break;
            }

            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip>(bp =>
            {
                bp.NoFatigue = nofatigue;
                bp.m_Type = Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip.SkipType.TimeOfDay;
                bp.MatchTimeOfDay = true;
                bp.TimeOfDay = time;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit TranslocateAction(Companions unit, FakeLocator position)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>(bp =>
            {
                bp.Unit = CommandTools.getCompanionEvaluator(unit);
                bp.translocatePositionEvaluator = position;
                bp.m_CopyRotation = true;
                bp.translocateOrientationEvaluator = position.GetRotation();
            });
            return result;
        }
        /*public static Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit TranslocateAction(Companions unit, Kingmaker.Blueprints.EntityReference position, bool setrotation = true)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>(bp =>
            {
                bp.Unit = CommandTools.getCompanionEvaluator(unit);
                bp.translocatePosition = position;
                bp.m_CopyRotation = setrotation;
            });
            return result;
        }*/

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit HideUnitAction(Companions unit, bool unhide = false)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit>(bp =>
            {
                bp.Target = CommandTools.getCompanionEvaluator(unit);
                bp.Unhide = unhide;
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene PlayCutsceneAction(Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene, SimpleBlueprint owner = null)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(cutscene);
                bp.Owner = owner;
                bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            });
            return action;
        }
        public static void CutsceneActionAddParameter(Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, string type, Kingmaker.ElementsSystem.Evaluator eval)
        {
            Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Float;
            switch (type)
            {
                case "Unit":
                case "unit":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Unit;
                    break;
                case "Locator":
                case "locator":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Locator;
                    break;
                case "MapObject":
                case "mapobject":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.MapObject;
                    break;
                case "Position":
                case "position":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Position;
                    break;
                case "Blueprint":
                case "blueprint":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Blueprint;
                    break;
                case "Float":
                case "float":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Float;
                    break;
            }
            CutsceneActionAddParameter(action, name, paramtype, eval);
        }
            public static void CutsceneActionAddParameter(Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType type, Kingmaker.ElementsSystem.Evaluator eval)
        {
            var parameter = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry();
            parameter.Name = name;
            parameter.Type = type;
            parameter.Evaluator = eval;

            var len = 0;
            if (action.Parameters.Parameters == null)
            {
                action.Parameters.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry[1];
            }
            else
            {
                len = action.Parameters.Parameters.Length;
                Array.Resize(ref action.Parameters.Parameters, len + 1);
            }
            action.Parameters.Parameters[len] = parameter;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene StopCutsceneAction(Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(cutscene);
            });
            return action;
        }


        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return StartEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(etude));
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return CompleteEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(etude));
        }

        public static Kingmaker.Kingdom.Actions.KingdomActionStartEvent StartCommandRoomEventAction(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionStartEvent>(bp =>
            {
                bp.m_Event = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintKingdomEventBaseReference>(commandevent);
                bp.m_Region = (BlueprintRegionReference)null;
            });
            return action;
        }
        public static Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent EndCommandRoomEventAction(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent>(bp =>
            {
                bp.m_EventBlueprint = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintKingdomEventBaseReference>(commandevent);
                bp.CancelIfInProgress = true;
                bp.AllIfMultiple = true;
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter AddCampEventAction(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter RemoveCampEventAction(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog StartDialogAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Companions owner)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog>(bp =>
            {
                bp.m_Dialogue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(dialog);
                bp.DialogueOwner = CommandTools.getCompanionEvaluator(owner);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic StopMusic()
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic>();
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic StartMusic(string trackname)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic>(bp =>
            {
                bp.MusicEventStart = "MUS_" + trackname + "_Play";
                bp.MusicEventStop = "MUS_" + trackname + "_Stop";
            });
            return action;
        }

        //Be warned, this is completely untested. Use at your own risk!
        public static SpawnUnit SpawnUnitAction(Kingmaker.Blueprints.BlueprintUnit unit, FakeLocator location)
        {
            var action = GenericAction<SpawnUnit>(bp =>
            {
                bp.unit = unit;
                bp.location = location;
            });
            return action;
        }

        //Creating a conditional action can take either a condition directly, for ease of creating simple checkers, or it can take a pre-constructed conditionchecker tree.
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(Kingmaker.ElementsSystem.Condition condition)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
            result.ConditionsChecker = new Kingmaker.ElementsSystem.ConditionsChecker();
            ConditionalTools.CheckerAddCondition(result.ConditionsChecker, condition);
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(Kingmaker.ElementsSystem.ConditionsChecker conditionchecker)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
            result.ConditionsChecker = conditionchecker;
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        // Because of the multitude of scenarios for actions in the true and false list, I thought it easier to not try to include them in the constructor.
        // Instead, you just have to call a couple additional functions.
        public static void ConditionalActionOnTrue(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfTrue = actionlist;
        }
        public static void ConditionalActionOnTrue(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfTrue == DialogTools.EmptyActionList)
            {//Make a brand new action list
                logicaction.IfTrue = new Kingmaker.ElementsSystem.ActionList();
            }
            var currentlen = 0;
            var paramlen = actions.Length;
            if (logicaction.IfTrue.Actions == null)
            {
                logicaction.IfTrue.Actions = new Kingmaker.ElementsSystem.GameAction[paramlen];
            }
            else
            {
                currentlen = logicaction.IfTrue.Actions.Length;
                Array.Resize(ref logicaction.IfTrue.Actions, currentlen + paramlen);
            }
            for (int i = 0; i<paramlen; i++)
            {
                logicaction.IfTrue.Actions[currentlen + i] = actions[i];
            }
        }
        public static void ConditionalActionOnFalse(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfFalse = actionlist;
        }
        public static void ConditionalActionOnFalse(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfFalse == DialogTools.EmptyActionList)
            {//Make a brand new action list
                logicaction.IfFalse = new Kingmaker.ElementsSystem.ActionList();
            }
            var currentlen = 0;
            var paramlen = actions.Length;
            if (logicaction.IfFalse.Actions == null)
            {
                logicaction.IfFalse.Actions = new Kingmaker.ElementsSystem.GameAction[paramlen];
            }
            else
            {
                currentlen = logicaction.IfFalse.Actions.Length;
                Array.Resize(ref logicaction.IfFalse.Actions, currentlen + paramlen);
            }
            for (int i = 0; i < paramlen; i++)
            {
                logicaction.IfFalse.Actions[currentlen + i] = actions[i];
            }
        }
    }
}