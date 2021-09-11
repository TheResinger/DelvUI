﻿using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.Jobs
{
    public class AstrologianHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new AstrologianConfig Config => (AstrologianConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;

        public AstrologianHud(string id, AstrologianConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            if (Config.ShowDivinationBar)
            {
                DrawDivinationBar(origin);
            }

            if (Config.ShowDrawBar)
            {
                DrawDraw(origin);
            }

            if (Config.ShowDotBar)
            {
                DrawDot(origin);
            }

            if (Config.ShowLightspeedBar)
            {
                DrawLightspeed(origin);
            }

            if (Config.ShowStarBar)
            {
                DrawStar(origin);
            }
        }

        protected string RedrawText(float redrawCastInfo, int redrawStacks)
        {
            if (!Config.ShowRedrawBar)
            {
                return "";
            }

            if (redrawCastInfo <= 0)
            {
                return Config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
            }

            if (!Config.EnableRedrawCooldownCumulated)
            {
                if (redrawCastInfo % 30 == 0)
                {
                    return "30";
                }

                redrawCastInfo %= 30;
            }

            if (Config.ShowRedrawCooldownTextBar)
            {
                string format = Config.EnableDecimalRedrawBar ? "N1" : "N0";

                return Config.ShowRedrawTextBar ? redrawCastInfo.ToString(format) + " [" + redrawStacks + "]" : redrawCastInfo.ToString(format);
            }

            return Config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
        }

        private unsafe void DrawDivinationBar(Vector2 origin)
        {
            List<Dictionary<string, uint>> chunkColors = new();

            ASTGauge gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            FieldInfo field = typeof(ASTGauge).GetField("seals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

            string textSealReady = "";
            int sealNumbers = 0;
            object result = field?.GetValue(gauge);
            GCHandle hdl = GCHandle.Alloc(result, GCHandleType.Pinned);
            byte* p = (byte*)hdl.AddrOfPinnedObject();

            for (int ix = 0; ix < 3; ++ix)
            {
                byte seal = *(p + ix);
                SealType type = (SealType)seal;

                switch (type)
                {
                    case SealType.NONE:
                        chunkColors.Add(EmptyColor);

                        break;

                    case SealType.MOON:
                        chunkColors.Add(Config.SealLunarColor.Map);

                        break;

                    case SealType.SUN:
                        chunkColors.Add(Config.SealSunColor.Map);

                        break;

                    case SealType.CELESTIAL:
                        chunkColors.Add(Config.SealCelestialColor.Map);

                        break;
                }

                if (gauge.ContainsSeal(SealType.NONE))
                {
                    continue;
                }

                sealNumbers = 0;

                if (gauge.ContainsSeal(SealType.SUN))
                {
                    sealNumbers++;
                }

                if (gauge.ContainsSeal(SealType.MOON))
                {
                    sealNumbers++;
                }

                if (gauge.ContainsSeal(SealType.CELESTIAL))
                {
                    sealNumbers++;
                }

                textSealReady = sealNumbers.ToString();
            }

            hdl.Free();
            float xPos = origin.X + Config.Position.X + Config.DivinationBarPosition.X - Config.DivinationBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DivinationBarPosition.Y - Config.DivinationBarSize.Y / 2f;

            BarBuilder bar = BarBuilder.Create(xPos, yPos, Config.DivinationBarSize.Y, Config.DivinationBarSize.X)
                                       .SetBackgroundColor(EmptyColor["background"])
                                       .SetChunks(3)
                                       .SetChunkPadding(Config.DivinationBarPad)
                                       .AddInnerBar(chunkColors.Count(n => n != EmptyColor), 3, chunkColors.ToArray())
                                       .SetTextMode(BarTextMode.Single)
                                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

            if (!Config.ShowDivinationTextBar)
            {
                textSealReady = "";
            }

            bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

            if (Config.ShowDivinationGlowBar)
            {
                bool[] chucksToGlow = new bool[3];

                for (int i = 0; i < sealNumbers; i++)
                {
                    chucksToGlow[i] = true;
                }

                bar.SetGlowChunks(chucksToGlow);
                bar.SetGlowColor(Config.DivinationGlowColor.Map["base"]);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList);
        }

        private void DrawDraw(Vector2 origin)
        {
            ASTGauge gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            float xPos = origin.X + Config.Position.X + Config.DrawBarPosition.X - Config.DrawBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DrawBarPosition.Y - Config.DrawBarSize.Y / 2f;

            string cardJob = "";
            Dictionary<string, uint> cardColor = EmptyColor;
            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.DrawBarSize.Y, Config.DrawBarSize.X);

            switch (gauge.DrawnCard())
            {
                case CardType.BALANCE:
                    cardColor = Config.SealSunColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.BOLE:
                    cardColor = Config.SealSunColor.Map;
                    cardJob = "RANGED";

                    break;

                case CardType.ARROW:
                    cardColor = Config.SealLunarColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.EWER:
                    cardColor = Config.SealLunarColor.Map;
                    cardJob = "RANGED";

                    break;

                case CardType.SPEAR:
                    cardColor = Config.SealCelestialColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.SPIRE:
                    cardColor = Config.SealCelestialColor.Map;
                    cardJob = "RANGED";

                    break;
            }

            float cardPresent;
            float cardMax;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            float drawCastInfo = _spellHelper.GetSpellCooldown(3590);
            float redrawCastInfo = _spellHelper.GetSpellCooldown(3593);
            int redrawStacks = _spellHelper.GetStackCount(3, 3593);

            if (cardJob != "")
            {
                cardPresent = 1f;
                cardMax = 1f;
            }
            else
            {
                cardPresent = drawCastInfo > 0 ? drawCastInfo : 1f;

                cardJob = drawCastInfo > 0 ? Math.Abs(drawCastInfo).ToString(Config.EnableDecimalDrawBar ? "N1" : "N0") : "READY";

                cardColor = drawCastInfo > 0 ? Config.DrawCdColor.Map : Config.DrawCdReadyColor.Map;
                cardMax = drawCastInfo > 0 ? 30f : 1f;
            }

            BarBuilder bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetTextMode(BarTextMode.Single)
                                    .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? Math.Abs(cardPresent).ToString("G") : "");

            if (Config.ShowDrawGlowBar)
            {
                switch (cardJob)
                {
                    case "RANGED":
                        bar.SetGlowColor(Config.DrawRangedGlowColor.Map["base"]);

                        break;

                    case "MELEE":
                        bar.SetGlowColor(Config.DrawMeleeGlowColor.Map["base"]);

                        break;
                }
            }

            if (!Config.ShowDrawTextBar)
            {
                if (!(cardJob == "RANGED" || cardJob == "MELEE" || cardJob == "READY"))
                {
                    cardJob = "";
                }
            }

            switch (cardJob)
            {
                case "RANGED":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                case "MELEE":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                case "READY":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                default:
                    bar.SetText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? cardJob : "");

                    break;
            }

            if (Config.ShowRedrawBar)
            {
                string redrawText = RedrawText(redrawCastInfo, redrawStacks);
                bar.AddPrimaryText(new BarText(BarTextPosition.CenterRight, BarTextType.Custom, redrawText));
            }

            bar.Build().Draw(drawList);
        }

        private void DrawDot(Vector2 origin)
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            float xPos = origin.X + Config.Position.X + Config.DotBarPosition.X - Config.DotBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DotBarPosition.Y - Config.DotBarSize.Y / 2f;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.DotBarSize.Y, Config.DotBarSize.X);

            if (target is not Chara)
            {
                Bar barNoTarget = builder.AddInnerBar(0, 30f, Config.DotColor.Map)
                                         .SetBackgroundColor(EmptyColor["background"])
                                         .SetTextMode(BarTextMode.Single)
                                         .SetText(
                                             BarTextPosition.CenterMiddle,
                                             BarTextType.Custom,
                                             Config.ShowDotTextBar
                                                 ? !Config.EnableDecimalDotBar
                                                     ? "0"
                                                     : "0.0"
                                                 : ""
                                         )
                                         .Build();

                barNoTarget.Draw(drawList);

                return;
            }

            StatusEffect dot = target.StatusEffects.FirstOrDefault(
                o => o.EffectId == 1881 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 843 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 838 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            float dotCooldown = dot.EffectId == 838 ? 18f : 30f;
            float dotDuration = Config.EnableDecimalDotBar ? dot.Duration : Math.Abs(dot.Duration);

            Bar bar = builder.AddInnerBar(dotDuration, dotCooldown, Config.DotColor.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .SetTextMode(BarTextMode.Single)
                             .SetText(
                                 BarTextPosition.CenterMiddle,
                                 BarTextType.Custom,
                                 Config.ShowDotTextBar
                                     ? !Config.EnableDecimalDotBar
                                         ? dot.Duration.ToString("N0")
                                         : Math.Abs(dot.Duration).ToString("N1")
                                     : ""
                             )
                             .Build();

            bar.Draw(drawList);
        }

        private void DrawLightspeed(Vector2 origin)
        {
            List<StatusEffect> lightspeedBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 841).ToList();
            float lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            float xPos = origin.X + Config.Position.X + Config.LightspeedBarPosition.X - Config.LightspeedBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.LightspeedBarPosition.Y - Config.LightspeedBarSize.Y / 2f;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().Duration);
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.LightspeedBarSize.Y, Config.LightspeedBarSize.X);

            Bar bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, Config.LightspeedColor.Map)
                             .SetTextMode(BarTextMode.Single)
                             .SetBackgroundColor(EmptyColor["background"])
                             .SetFlipDrainDirection(true)
                             .SetText(
                                 BarTextPosition.CenterMiddle,
                                 BarTextType.Custom,
                                 Config.ShowLightspeedTextBar
                                     ? !Config.EnableDecimalLightspeedBar
                                         ? lightspeedDuration.ToString("N0")
                                         : Math.Abs(lightspeedDuration).ToString("N1")
                                     : ""
                             )
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawStar(Vector2 origin)
        {
            List<StatusEffect> starPreCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1224).ToList();

            List<StatusEffect> starPostCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1248).ToList();

            float starDuration = 0f;
            const float starMaxDuration = 10f;

            float xPos = origin.X + Config.Position.X + Config.StarBarPosition.X - Config.StarBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.StarBarPosition.Y - Config.StarBarSize.Y / 2f;
            Dictionary<string, uint> starColorSelector = EmptyColor;

            if (starPreCookingBuff.Any())
            {
                starDuration = starMaxDuration - Math.Abs(starPreCookingBuff.First().Duration);
                starColorSelector = Config.StarEarthlyColor.Map;
            }

            if (starPostCookingBuff.Any())
            {
                starDuration = Math.Abs(starPostCookingBuff.First().Duration);
                starColorSelector = Config.StarGiantColor.Map;
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.StarBarSize.Y, Config.StarBarSize.X);

            BarBuilder bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                                    .SetTextMode(BarTextMode.Single)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetText(
                                        BarTextPosition.CenterMiddle,
                                        BarTextType.Custom,
                                        Config.ShowStarTextBar
                                            ? !Config.EnableDecimalStarBar
                                                ? starDuration.ToString("N0")
                                                : Math.Abs(starDuration).ToString("N1")
                                            : ""
                                    );

            if (starColorSelector == Config.StarGiantColor.Map && Config.ShowStarGlowBar)
            {
                bar.SetGlowColor(Config.StarGlowColor.Map["base"]);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Astrologian", 1)]
    public class AstrologianConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.AST;
        public new static AstrologianConfig DefaultConfig() { return new AstrologianConfig(); }

        #region Draw Bar
        [Checkbox("Show Draw Bar")]
        [CollapseControl(30, 0)]
        public bool ShowDrawBar = true;

        [DragFloat2("Draw Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 DrawBarSize = new(254, 20);

        [DragFloat2("Draw Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 DrawBarPosition = new(0, HUDConstants.JobHudsBaseY - 10);

        [ColorEdit4("Draw on CD Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor DrawCdColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

        [ColorEdit4("Draw Ready Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor DrawCdReadyColor = new(new Vector4(137f / 255f, 26f / 255f, 42f / 255f, 100f / 100f));

        [ColorEdit4("Draw Melee Glow Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor DrawMeleeGlowColor = new(new Vector4(83f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [ColorEdit4("Draw Ranged Glow Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor DrawRangedGlowColor = new(new Vector4(124f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [Checkbox("Show card preferred target with glow")]
        [CollapseWith(30, 0)]
        public bool ShowDrawGlowBar;

        [Checkbox("Show card preferred target with text")]
        [CollapseWith(35, 0)]
        public bool ShowDrawTextBar = true;

        [Checkbox("Show Draw Timer")]
        [CollapseWith(40, 0)]
        public bool ShowDrawCooldownTextBar = true;

        [Checkbox("Enable Redraw Stacks & Cooldown")]
        [CollapseWith(45, 0)]
        public bool ShowRedrawBar = true;

        [Checkbox("Show Redraw Timer")]
        [CollapseWith(50, 0)]
        public bool ShowRedrawCooldownTextBar = true;

        [Checkbox("Show Redraw Stacks")]
        [CollapseWith(55, 0)]
        public bool ShowRedrawTextBar = true;

        [Checkbox("Change next Redraw cooldown to total Redraw cooldown")]
        [CollapseWith(60, 0)]
        public bool EnableRedrawCooldownCumulated;

        [Checkbox("Change Draw timer to decimal")]
        [CollapseWith(65, 0)]
        public bool EnableDecimalDrawBar;

        [Checkbox("Change Redraw timer to decimal")]
        [CollapseWith(70, 0)]
        public bool EnableDecimalRedrawBar;
        #endregion

        #region Divination Bar
        [Checkbox("Show Divination Bar")]
        [CollapseControl(35, 1)]
        public bool ShowDivinationBar = true;

        [DragFloat2("Divination Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DivinationBarSize = new(254, 10);

        [DragFloat2("Divination Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 DivinationBarPosition = new(0, HUDConstants.JobHudsBaseY - 49);

        [DragInt("Divination Bar Padding", min = -1000, max = 1000)]
        [CollapseWith(10, 1)]
        public int DivinationBarPad = 2;

        [ColorEdit4("Seal Sun Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor SealSunColor = new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

        [ColorEdit4("Seal Lunar Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor SealLunarColor = new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

        [ColorEdit4("Seal Celestial Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor SealCelestialColor = new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

        [ColorEdit4("Divination Glow Color")]
        [CollapseWith(30, 1)]
        public PluginConfigColor DivinationGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        [Checkbox("Show numbers of different seals for Divination with glow")]
        [CollapseWith(35, 1)]
        public bool ShowDivinationGlowBar = true;

        [Checkbox("Show numbers of different seals for Divination with text")]
        [CollapseWith(40, 1)]
        public bool ShowDivinationTextBar;
        #endregion

        #region Dot Bar
        [Checkbox("Show Dot Bar")]
        [CollapseControl(40, 2)]
        public bool ShowDotBar = true;

        [DragFloat2("Dot Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 DotBarSize = new(84, 20);

        [DragFloat2("Dot Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 DotBarPosition = new(-85, HUDConstants.JobHudsBaseY - 32);

        [ColorEdit4("Dot Color")]
        [CollapseWith(10, 2)]
        public PluginConfigColor DotColor = new(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 100f / 100f));

        [Checkbox("Show Dot timer")]
        [CollapseWith(15, 2)]
        public bool ShowDotTextBar = true;

        [Checkbox("Change Dot timer to decimal")]
        [CollapseWith(20, 2)]
        public bool EnableDecimalDotBar;
        #endregion

        #region Star Bar
        [Checkbox("Show Star Bar")]
        [CollapseControl(45, 3)]
        public bool ShowStarBar = true;

        [DragFloat2("Star Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 3)]
        public Vector2 StarBarSize = new(84, 20);

        [DragFloat2("Star Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 StarBarPosition = new(0, HUDConstants.JobHudsBaseY - 32);

        [ColorEdit4("Star Earthly Color")]
        [CollapseWith(10, 3)]
        public PluginConfigColor StarEarthlyColor = new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("Star Giant Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor StarGiantColor = new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

        [ColorEdit4("Star Glow Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor StarGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        [Checkbox("Show Star timer")]
        [CollapseWith(25, 3)]
        public bool ShowStarTextBar = true;

        [Checkbox("Change Star timer to decimal")]
        [CollapseWith(30, 3)]
        public bool EnableDecimalStarBar;

        [Checkbox("Enable Star bar glow when Giant Dominance is ready")]
        [CollapseWith(35, 3)]
        public bool ShowStarGlowBar = true;
        #endregion

        #region Lightspeed Bar
        [Checkbox("Show Lightspeed Bar")]
        [CollapseControl(50, 4)]
        public bool ShowLightspeedBar = true;

        [DragFloat2("Lightspeed Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 4)]
        public Vector2 LightspeedBarSize = new(84, 20);

        [DragFloat2("Lightspeed Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 LightspeedBarPosition = new(85, HUDConstants.JobHudsBaseY - 32);

        [ColorEdit4("Lightspeed Color")]
        [CollapseWith(10, 4)]
        public PluginConfigColor LightspeedColor = new(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f));

        [Checkbox("Show Lightspeed timer")]
        [CollapseWith(15, 4)]
        public bool ShowLightspeedTextBar = true;

        [Checkbox("Change Lightspeed timer to decimal")]
        [CollapseWith(20, 4)]
        public bool EnableDecimalLightspeedBar;
        #endregion
    }
}