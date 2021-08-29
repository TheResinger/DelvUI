﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class ConfigurationWindow
    {
        public bool IsVisible;
        private readonly Plugin _plugin;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly PluginConfiguration _pluginConfiguration;
        private string selected = "Individual Unitframes";
        private Dictionary<string, Array> configMap = new Dictionary<string, Array>() ;
        private bool changed;
        private int viewportWidth = (int) ImGui.GetMainViewport().Size.X;
        private int viewportHeight = (int) ImGui.GetMainViewport().Size.Y;
        private int xOffsetLimit;
        private int yOffsetLimit;

        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            //TODO ADD PRIMARYRESOURCEBAR TO CONFIGMAP jobs general

            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
            //configMap.Add("General", new [] {"General"});
            configMap.Add("Individual Unitframes", new [] {"General","Colors", "Shields", "Player", "Target", "Target of Target", "Focus"});
            //configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            configMap.Add("Castbars", new [] {
                //"General", 
                "Player"
                //, "Enemy"
                });
            configMap.Add("Jobs", new [] {"General", "Tank", "Healer", "Melee","Ranged", "Caster"});

        }   


        public void Draw()
        {
            if (!IsVisible) {
                return;
            }
            //Todo future reference dalamud native ui scaling
            //ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);


            if (!ImGui.Begin("titlebar", ref IsVisible, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse)) {
                return;
            }
            xOffsetLimit = viewportWidth / 2;
            yOffsetLimit = viewportHeight / 2;
            changed = false;
            var pos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth()-26, 0));
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Times.ToIconString()))
            {
                IsVisible = false;
            }
            ImGui.PopFont();
            ImGui.SetCursorPos(pos);
            
            ImGui.BeginGroup();
            {
                ImGui.BeginGroup(); // Left
                {
                    var imagePath = Path.Combine(Path.GetDirectoryName(_plugin.AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");
                    var delvuiBanner = _pluginInterface.UiBuilder.LoadImage(imagePath);
                    ImGui.Image(delvuiBanner.ImGuiHandle, new Vector2(delvuiBanner.Width, delvuiBanner.Height));

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing()), true);

                    foreach (var config in configMap.Keys)
                    {
                        if (ImGui.Selectable(config, selected == config))
                            selected = config;
                    }

                    ImGui.EndChild();


                }
                ImGui.EndGroup();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();
                {
                    var subConfigs = configMap[selected];
                        
                    ImGui.BeginChild("item view",new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
                    {
                        if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                        {
                            foreach (string subConfig in subConfigs)
                            {

                                if (!ImGui.BeginTabItem(subConfig)) continue;
                                ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                                DrawSubConfig(selected, subConfig);
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                                
                        }
                    }
                    ImGui.EndChild();
                        
                }
                ImGui.EndGroup();
            }
            ImGui.EndGroup();
            ImGui.Separator();

            ImGui.BeginGroup();

            if (ImGui.Button("Lock HUD"))
            {
                changed |= ImGui.Checkbox("Lock HUD", ref _pluginConfiguration.LockHud);

            }
            ImGui.SameLine();
            if (ImGui.Button("Hide HUD"))
            {
                changed |= ImGui.Checkbox("Hide HUD", ref _pluginConfiguration.HideHud);

            }                
            ImGui.SameLine();
            if (ImGui.Button("Reset HUD")) {}
            ImGui.SameLine();
            
            pos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth()-60, ImGui.GetCursorPos().Y));
            if (ImGui.Button("Donate!"))
            {
            }
            ImGui.SetCursorPos(pos);
            ImGui.EndGroup();
                

            if (changed)
            {
                _pluginConfiguration.BuildColorMap();
                _pluginConfiguration.Save();
            }
            
            ImGui.End();
        }
        private void DrawSubConfig(string config, string subConfig)
        {
            switch (config)
            {
                case "General":
                    switch (subConfig)
                    {
                        case "General":
                            DrawGeneralGeneralConfig();
                            break;

                    }
                    break;
                case "Individual Unitframes":
                    switch (subConfig)
                    {
                        //TODO NEST COLOR MAP AND SHIELDS ON GENERAL
                        case "General":
                            DrawIndividualUnitFramesGeneralConfig();
                            break;   
                        case "Colors":
                            DrawGeneralColorMapConfig();
                            break;
                        case "Shields":
                            DrawIndividualUnitFramesShieldsConfig();
                            break;
                        case "Player":
                            DrawIndividualUnitFramesPlayerConfig();
                            break;
                        case "Target":
                            DrawIndividualUnitFramesTargetConfig();
                            break;                        
                        case "Target of Target":
                            DrawIndividualUnitFramesToTConfig();
                            break;                        
                        case "Focus":
                            DrawIndividualUnitFramesFocusConfig();
                            break;
                    }
                    break;
                case "Group Unitframes":
                    switch (subConfig)
                    {
                        case "General":
                            DrawGroupUnitFramesGeneralConfig();
                            break;
                        case "Party":
                            DrawGroupUnitFramesPartyConfig();
                            break;                        
                        case "8man":
                            DrawGroupUnitFrames8manConfig();
                            break;                        
                        case "24man":
                            DrawGroupUnitFrames24manConfig();
                            break;                        
                        case "Enemies":
                            DrawGroupUnitFramesEnemiesConfig();
                            break;
                    }
                    break;
                case "Castbars":
                    switch (subConfig)
                    {
                        case "General":
                            DrawCastbarsGeneralConfig();
                            break;
                        case "Player":
                            DrawCastbarsPlayerConfig();
                            break;
                        case "Enemy":
                            DrawCastbarsEnemyConfig();
                            break;
                    }
                    break;
                case "Jobs":
                    switch (subConfig)
                    {          
                        case "General":
                            DrawJobsGeneralConfig();
                            break;
                        case "Tank":
                            DrawJobsTankConfig();
                            break;                        
                        case "Healer":  
                            DrawJobsHealerConfig();
                            break;                        
                        case "Melee":
                            DrawJobsMeleeConfig();
                            break;                        
                        case "Ranged":
                            DrawJobsRangedConfig();
                            break;                        
                        case "Caster":
                            DrawJobsCasterConfig();
                            break;
                    }
                    break;
            }
            
        }

        private void DrawGeneralGeneralConfig()
        {
            ImGui.Text("this has no configs yet");

        }

        private void DrawGeneralColorMapConfig()
        {
            if (ImGui.BeginTabBar("##ColorTabs", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Tanks"))
                {                            
                    ImGui.Text("");//SPACING
                    ImGui.Text("Paladin");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorPLD);
                    
                    ImGui.Text("");//SPACING
                    ImGui.Text("Warrior");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorWAR);
                    
                    ImGui.Text("");//SPACING
                    ImGui.Text("Dark Knight");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorDRK);
                    
                    ImGui.Text("");//SPACING
                    ImGui.Text("Gunbreaker");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorGNB);
                    
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Healers"))
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("White Mage");
                    changed |= ImGui.ColorEdit4(" ", ref _pluginConfiguration.JobColorWHM);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Scholar");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorSCH);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Astrologian");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorAST);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Melee"))
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Monk");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorMNK);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Dragoon");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorDRG);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Ninja");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorNIN);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Samurai");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorSAM);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Ranged"))
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Bard");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorBRD);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Machinist");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorMCH);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Dancer");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorDNC);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Casters"))
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Black Mage");
                    changed |= ImGui.ColorEdit4(" ", ref _pluginConfiguration.JobColorBLM);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Summoner");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.JobColorSMN);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Red Mage");
                    changed |= ImGui.ColorEdit4(" ", ref _pluginConfiguration.JobColorRDM);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Blue Mage");
                    changed |= ImGui.ColorEdit4(" ", ref _pluginConfiguration.JobColorBLU);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("NPC"))
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Hostile");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.NPCColorHostile);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Neutral");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.NPCColorNeutral);
                    ImGui.Text("");//SPACING
                    ImGui.Text("Friendly");
                    changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.NPCColorFriendly);
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            
        }
        private void DrawLeftRightConfigBoxes(){}
        private void DrawIndividualUnitFramesGeneralConfig()
        {
            //TODO NEST COLOR MAP AND SHIELDS ON GENERAL
            ImGui.Text("this has no configs yet");
        }
        
        private void DrawIndividualUnitFramesShieldsConfig()
        {
            changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShieldEnabled);

            var shieldHeight = _pluginConfiguration.ShieldHeight;
            if (ImGui.DragInt("Height", ref shieldHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.ShieldHeight = shieldHeight;
                _pluginConfiguration.Save();
            }

            changed |= ImGui.Checkbox("Height in px", ref _pluginConfiguration.ShieldHeightPixels);

            changed |= ImGui.ColorEdit4("Color", ref _pluginConfiguration.ShieldColor);

        }
        
        private void DrawIndividualUnitFramesPlayerConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var healthBarHeight = _pluginConfiguration.HealthBarHeight;
                            if (ImGui.DragInt("", ref healthBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarHeight = healthBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var healthBarXOffset = _pluginConfiguration.HealthBarXOffset;
                            if (ImGui.DragInt("", ref healthBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarXOffset = healthBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var healthBarWidth = _pluginConfiguration.HealthBarWidth;
                            if (ImGui.DragInt("", ref healthBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarWidth = healthBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var healthBarYOffset = _pluginConfiguration.HealthBarYOffset;
                            if (ImGui.DragInt("", ref healthBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarYOffset = healthBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text("");//SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Format");
                            var healthBarTextLeft = _pluginConfiguration.HealthBarTextLeft;
                            if (ImGui.InputText("", ref healthBarTextLeft, 999))
                            {
                                _pluginConfiguration.HealthBarTextLeft = healthBarTextLeft;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text X Offset");
                            var healthBarTextLeftXOffset = _pluginConfiguration.HealthBarTextLeftXOffset;
                            if (ImGui.DragInt("", ref healthBarTextLeftXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftXOffset = healthBarTextLeftXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Y Offset");
                            var healthBarTextLeftYOffset = _pluginConfiguration.HealthBarTextLeftYOffset;
                            if (ImGui.DragInt("", ref healthBarTextLeftYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftYOffset = healthBarTextLeftYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Format");
                            var healthBarTextRight = _pluginConfiguration.HealthBarTextRight;
                            if (ImGui.InputText("", ref healthBarTextRight, 999))
                            {
                                _pluginConfiguration.HealthBarTextRight = healthBarTextRight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text X Offset");
                            var healthBarTextRightXOffset = _pluginConfiguration.HealthBarTextRightXOffset;
                            if (ImGui.DragInt("", ref healthBarTextRightXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextRightXOffset = healthBarTextRightXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Y Offset");
                            var healthBarTextRightYOffset = _pluginConfiguration.HealthBarTextRightYOffset;
                            if (ImGui.DragInt("", ref healthBarTextRightYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextRightYOffset = healthBarTextRightYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        } 
        private void DrawIndividualUnitFramesTargetConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var TargetBarHeight = _pluginConfiguration.TargetBarHeight;
                            if (ImGui.DragInt("", ref TargetBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.TargetBarHeight = TargetBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var TargetBarXOffset = _pluginConfiguration.TargetBarXOffset;
                            if (ImGui.DragInt("", ref TargetBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarXOffset = TargetBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var TargetBarWidth = _pluginConfiguration.TargetBarWidth;
                            if (ImGui.DragInt("", ref TargetBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.TargetBarWidth = TargetBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var TargetBarYOffset = _pluginConfiguration.TargetBarYOffset;
                            if (ImGui.DragInt("", ref TargetBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarYOffset = TargetBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text("");//SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Format");
                            var TargetBarTextLeft = _pluginConfiguration.TargetBarTextLeft;
                            if (ImGui.InputText("", ref TargetBarTextLeft, 999))
                            {
                                _pluginConfiguration.TargetBarTextLeft = TargetBarTextLeft;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text X Offset");
                            var TargetBarTextLeftXOffset = _pluginConfiguration.TargetBarTextLeftXOffset;
                            if (ImGui.DragInt("", ref TargetBarTextLeftXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextLeftXOffset = TargetBarTextLeftXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Y Offset");
                            var TargetBarTextLeftYOffset = _pluginConfiguration.TargetBarTextLeftYOffset;
                            if (ImGui.DragInt("", ref TargetBarTextLeftYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextLeftYOffset = TargetBarTextLeftYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Format");
                            var TargetBarTextRight = _pluginConfiguration.TargetBarTextRight;
                            if (ImGui.InputText("", ref TargetBarTextRight, 999))
                            {
                                _pluginConfiguration.TargetBarTextRight = TargetBarTextRight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text X Offset");
                            var TargetBarTextRightXOffset = _pluginConfiguration.TargetBarTextRightXOffset;
                            if (ImGui.DragInt("", ref TargetBarTextRightXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextRightXOffset = TargetBarTextRightXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Y Offset");
                            var TargetBarTextRightYOffset = _pluginConfiguration.TargetBarTextRightYOffset;
                            if (ImGui.DragInt("", ref TargetBarTextRightYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextRightYOffset = TargetBarTextRightYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        } 
        private void DrawIndividualUnitFramesToTConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var ToTBarHeight = _pluginConfiguration.ToTBarHeight;
                            if (ImGui.DragInt("", ref ToTBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarHeight = ToTBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var ToTBarXOffset = _pluginConfiguration.ToTBarXOffset;
                            if (ImGui.DragInt("", ref ToTBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarXOffset = ToTBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var ToTBarWidth = _pluginConfiguration.ToTBarWidth;
                            if (ImGui.DragInt("", ref ToTBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarWidth = ToTBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var ToTBarYOffset = _pluginConfiguration.ToTBarYOffset;
                            if (ImGui.DragInt("", ref ToTBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarYOffset = ToTBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text("");//SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                //TODO FIX WIDTH LIKE CASTBAR, TEXT FORMAT 100% XY 50 50
                //TODO COPY PASTA DESIGN TO FOCUS BAR
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Text Format");
                            var ToTBarText = _pluginConfiguration.ToTBarText;
                            if (ImGui.InputText("", ref ToTBarText, 999))
                            {
                                _pluginConfiguration.ToTBarText = ToTBarText;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Text X Offset");
                            var ToTBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;
                            if (ImGui.DragInt("", ref ToTBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextXOffset = ToTBarTextXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Text Y Offset");
                            var ToTBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;
                            if (ImGui.DragInt("", ref ToTBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextYOffset = ToTBarTextYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        
                    }
                    ImGui.EndChild();
                    

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        } 
 
        private void DrawIndividualUnitFramesToTConfigss(){
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled);//TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Height");
                            var ToTBarHeight = _pluginConfiguration.ToTBarHeight;
                            if (ImGui.DragInt("", ref ToTBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarHeight = ToTBarHeight;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Width");
                            var ToTBarWidth = _pluginConfiguration.ToTBarWidth;
                            if (ImGui.DragInt("", ref ToTBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarWidth = ToTBarWidth;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hpoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("X Offset");
                            var ToTBarXOffset = _pluginConfiguration.ToTBarXOffset;
                            if (ImGui.DragInt("", ref ToTBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarXOffset = ToTBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Y Offset");
                            var ToTBarYOffset = _pluginConfiguration.ToTBarYOffset;
                            if (ImGui.DragInt("", ref ToTBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarYOffset = ToTBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text Format");
                            var ToTBarText = _pluginConfiguration.ToTBarText;
                            if (ImGui.InputText("", ref ToTBarText, 999))
                            {
                                _pluginConfiguration.ToTBarText = ToTBarText;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        

                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hptxtoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtleftxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text X Offset");
                            var ToTBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;
                            if (ImGui.DragInt("", ref ToTBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextXOffset = ToTBarTextXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtleftypane", new Vector2(ImGui.GetWindowWidth()/2, 0));
                        {
                            ImGui.Text("Text Y Offset");
                            var ToTBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;
                            if (ImGui.DragInt("", ref ToTBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextYOffset = ToTBarTextYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        }        
        private void DrawIndividualUnitFramesFocusConfig(){
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled);//TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Height");
                            var FocusBarHeight = _pluginConfiguration.FocusBarHeight;
                            if (ImGui.DragInt("", ref FocusBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarHeight = FocusBarHeight;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Width");
                            var FocusBarWidth = _pluginConfiguration.FocusBarWidth;
                            if (ImGui.DragInt("", ref FocusBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarWidth = FocusBarWidth;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hpoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("X Offset");
                            var FocusBarXOffset = _pluginConfiguration.FocusBarXOffset;
                            if (ImGui.DragInt("", ref FocusBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarXOffset = FocusBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Y Offset");
                            var FocusBarYOffset = _pluginConfiguration.FocusBarYOffset;
                            if (ImGui.DragInt("", ref FocusBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarYOffset = FocusBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text Format");
                            var FocusBarText = _pluginConfiguration.FocusBarText;
                            if (ImGui.InputText("", ref FocusBarText, 999))
                            {
                                _pluginConfiguration.FocusBarText = FocusBarText;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        

                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hptxtoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtleftxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text X Offset");
                            var FocusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;
                            if (ImGui.DragInt("", ref FocusBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarTextXOffset = FocusBarTextXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtleftypane", new Vector2(ImGui.GetWindowWidth()/2, 0));
                        {
                            ImGui.Text("Text Y Offset");
                            var FocusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;
                            if (ImGui.DragInt("", ref FocusBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextYOffset = FocusBarTextYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        }        
        private void DrawCastbarsPlayerConfig(){
            bool disabled = true;
            changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShowCastBar);

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var CastBarHeight = _pluginConfiguration.CastBarHeight;
                            if (ImGui.DragInt("", ref CastBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.CastBarHeight = CastBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var CastBarXOffset = _pluginConfiguration.CastBarXOffset;
                            if (ImGui.DragInt("", ref CastBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.CastBarXOffset = CastBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var CastBarWidth = _pluginConfiguration.CastBarWidth;
                            if (ImGui.DragInt("", ref CastBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.CastBarWidth = CastBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var CastBarYOffset = _pluginConfiguration.CastBarYOffset;
                            if (ImGui.DragInt("", ref CastBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.CastBarYOffset = CastBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Text("");//SPACING

                    ImGui.BeginChild("castbarcolor", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("Color");
                        changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.CastBarColor);

                        
                    }
                    ImGui.EndChild();
                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();   
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Other Options");
                ImGui.BeginChild("otheroptions", new Vector2(0,ImGui.GetWindowHeight()/4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("otheroptions1", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("otheroptions2", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Interrupted", ref _pluginConfiguration.InterruptCheck);
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Action Icon", ref _pluginConfiguration.ShowActionIcon);
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("otheroptions3", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Action Name", ref _pluginConfiguration.ShowActionName);
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Cast Time", ref _pluginConfiguration.ShowCastTime);
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {                        
                changed |= ImGui.Checkbox("SlideCast", ref _pluginConfiguration.SlideCast);
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth(), 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            ImGui.Text("Offset");
                            var SlideCastTime = _pluginConfiguration.SlideCastTime;
                            if (ImGui.DragFloat("", ref SlideCastTime, 1, 1, 1000))
                            {
                                _pluginConfiguration.SlideCastTime = SlideCastTime;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING
                            ImGui.Text("Color");
                            changed |= ImGui.ColorEdit4("", ref _pluginConfiguration.SlideCastColor);

                        }
                        ImGui.EndChild();
                        

                        
                    }
                    ImGui.EndChild();
                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
        }        
        

     
        

        private void DrawGroupUnitFramesGeneralConfig()
        {
            
        }

        private void DrawGroupUnitFramesPartyConfig()
        {
            
        }

        private void DrawGroupUnitFrames8manConfig()
        {
            
        }

        private void DrawGroupUnitFrames24manConfig()
        {
            
        }

        private void DrawGroupUnitFramesEnemiesConfig()
        {
            
        }

        private void DrawCastbarsGeneralConfig()
        {
            
        }

        private void DrawCastbarsPlayerConfigss()
        {

                    var castBarHeight = _pluginConfiguration.CastBarHeight;
                    if (ImGui.DragInt("Castbar Height", ref castBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.CastBarHeight = castBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var castBarWidth = _pluginConfiguration.CastBarWidth;
                    if (ImGui.DragInt("Castbar Width", ref castBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.CastBarWidth = castBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var castBarXOffset = _pluginConfiguration.CastBarXOffset;
                    if (ImGui.DragInt("Castbar X Offset", ref castBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.CastBarXOffset = castBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var castBarYOffset = _pluginConfiguration.CastBarYOffset;
                    if (ImGui.DragInt("Castbar Y Offset", ref castBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.CastBarYOffset = castBarYOffset;
                        _pluginConfiguration.Save();
                    }






                    var slideCastTime = _pluginConfiguration.SlideCastTime;
                    if (ImGui.DragFloat("Slide Cast Offset", ref slideCastTime, 1, 1, 1000))
                    {
                        _pluginConfiguration.SlideCastTime = slideCastTime;
                        _pluginConfiguration.Save();
                    }


        }

        private void DrawCastbarsEnemyConfig()
        {
            
        }

        private void DrawJobsGeneralConfig()
        {
            
        }

        private void DrawJobsTankConfig()
        {
            
        }

        private void DrawJobsHealerConfig()
        {
            
        }

        private void DrawJobsMeleeConfig()
        {
            
        }

        private void DrawJobsRangedConfig()
        {
            
        }

        private void DrawJobsCasterConfig()
        {
            
        }

    }
}