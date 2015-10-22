﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;
using KoPlayer.Forms;

namespace KoPlayer
{
    public class Settings
    {
        public int FormWidth = 1000;
        public int FormHeight = 600;

        private int rowHeight = 17;
        public int RowHeight { get { return rowHeight; }
            set 
            {
                if (rowHeight < 15)
                    rowHeight = 15;
                else
                    rowHeight = value;
            }
        }

        private string fontName = "Microsoft Sans Serif";
        public string FontName { get { return fontName; } set { fontName = value; } }

        private float fontSize = 8f;
        public float FontSize { get { return fontSize; }
            set
            {
                if (fontSize < 7f)
                    fontSize = 7f;
                else if (fontSize > 12f)
                    fontSize = 12f;
                else
                    fontSize = value;
            }
        }

        public string StartupPlayList = "Library";

        public string Partymix_SourcePlayListName = "Library";
        public int Partymix_NumPrevious = 5;
        public int Partymix_NumNext = 15;

        public Keys[] RatingHotkeys = { (Keys)220, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5 };

        public GlobalHotkey[] GlobalHotkeys = { new GlobalHotkey(GlobalHotkeyAction.IncreaseVolume, Keys.Insert, ModifierKeys.Control),
                                                new GlobalHotkey(GlobalHotkeyAction.DecreaseVolume, Keys.Delete, ModifierKeys.Control),
                                                new GlobalHotkey(GlobalHotkeyAction.PlayOrPause, Keys.Home, ModifierKeys.Control),
                                                new GlobalHotkey(GlobalHotkeyAction.ShowSongInfoPopup, Keys.End, ModifierKeys.Control),
                                                new GlobalHotkey(GlobalHotkeyAction.PlayPreviousSong, Keys.PageUp, ModifierKeys.Control),
                                                new GlobalHotkey(GlobalHotkeyAction.PlayNextSong, Keys.PageDown, ModifierKeys.Control)};
        [XmlIgnore]
        public string[] GlobalHotkeyNames = { "Increase volume", "Decrease volume", "Play / Pause",
                                            "Show song info popup", "Play previous song", "Play next song"};

        public Settings()
        {

        }

        /// <summary>
        /// Loads from file
        /// </summary>
        /// <param name="path"></param>
        public Settings(string path)
        {
            Settings loadedSettings = Load(path);
        }

        /// <summary>
        /// Saves the current settings
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        public void Save(string filename)
        {
            Stream stream = File.Create(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Loads settings from a file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public static Settings Load(string filename)
        {
            Stream stream = null;
            Settings loadedSettings = null;
            try
            {
                stream = File.OpenRead(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                loadedSettings = (Settings)serializer.Deserialize(stream);
                stream.Close();
                
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            return loadedSettings;
        }

        /*public static Settings Copy(Settings settings)
        {
            Settings ret = new Settings();
            ret.FontName = settings.FontName;
            ret.FontSize = settings.FontSize;
            ret.FormHeight = settings.FormHeight;
            ret.FormWidth = settings.FormWidth;
            ret.Partymix_NumNext = settings.Partymix_NumNext;
            ret.Partymix_NumPrevious = settings.Partymix_NumPrevious;
            ret.Partymix_SourcePlayListName = settings.Partymix_SourcePlayListName;
            ret.rowHeight = settings.rowHeight;
            ret.StartupPlayList = settings.StartupPlayList;
            return ret;
        }*/
    }

    public enum GlobalHotkeyAction
    {
        IncreaseVolume,
        DecreaseVolume,
        PlayOrPause,
        ShowSongInfoPopup,
        PlayPreviousSong,
        PlayNextSong,
    }

    public struct GlobalHotkey
    {
        public GlobalHotkeyAction Action;
        public Keys Key;
        public ModifierKeys Modifier;

        public GlobalHotkey(GlobalHotkeyAction action, Keys key, ModifierKeys modifier)
        {
            this.Action = action;
            this.Key = key;
            this.Modifier = modifier;
        }
    }
}
