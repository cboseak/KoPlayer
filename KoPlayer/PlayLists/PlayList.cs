﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.ComponentModel;

namespace KoPlayer.PlayLists
{

    public class PlayList : IPlayList
    {
        public static Random r = new Random();

        [XmlIgnore]
        public string Path { get { return @"Playlists\" + Name + ".xml"; } }
        public string Name { get; set; }
        [XmlIgnore]
        public int SortColumnIndex { get; set; }
        [XmlIgnore]
        public int NumSongs { get { return songPaths.Count; } }
        public int CurrentIndex { get; set; }

        private const string EXTENSION = ".mp3";

        private BindingList<Song> outputSongs;
        public List<string> songPaths;
        protected Dictionary<string, Song> libraryDictionary;

        public SortOrder SortOrder { get { return sortOrder; } }
        private SortOrder sortOrder;
        private string sortField = "";

        private List<Dictionary<string, List<Song>>> sortDictionaries;

        protected PlayList() { }

        public PlayList(Library library)
            : this(library, "PlayListName") { }

        public PlayList(Library library, string name)
            : this(library, name, new List<string>()) { }

        public PlayList(Library library, string name, List<string> songPaths)
        {
            this.libraryDictionary = library.Dictionary;
            library.LibraryChanged += library_LibraryChanged;
            this.Name = name;
            this.songPaths = songPaths;
            ResetSortVariables();

            this.outputSongs = GetSongsFromLibrary();
            this.sortDictionaries = new List<Dictionary<string, List<Song>>>();
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
        }

        private BindingList<Song> GetSongsFromLibrary()
        {
            BindingList<Song> ret = new BindingList<Song>();
            if (songPaths.Count == 0)
                return ret;
            foreach (string songPath in songPaths)
                if (libraryDictionary.ContainsKey(songPath))
                    ret.Add(libraryDictionary[songPath]);
            return ret;
        }

        void library_LibraryChanged(object sender, LibraryChangedEventArgs e)
        {
            List<int> toBeRemoved = new List<int>();
            for (int i = 0; i < songPaths.Count; i++)
            {
                if (!libraryDictionary.ContainsKey(songPaths[i]))
                    toBeRemoved.Add(i);
            }
            toBeRemoved.Sort();
            toBeRemoved.Reverse();
            foreach (int i in toBeRemoved)
                this.Remove(i);
        }

        public void Add(string path)
        {
            if (libraryDictionary.Keys.Contains(path))
                Add(libraryDictionary[path]);
        }

        public void Add(Song song)
        {
            if (song != null)
            {
                songPaths.Add(song.Path);
                this.outputSongs.Add(libraryDictionary[song.Path]);
                Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
            }
        }

        public void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public void Insert(int index, string song)
        {
            if (index < songPaths.Count)
                songPaths.Insert(index, song);
            else
                songPaths.Add(song);
        }

        public void Insert(int index, Song song)
        {
            if (song != null)
                this.Insert(index, song.Path);
        }

        public void Insert(int index, List<Song> songs)
        {
            foreach (Song s in songs)
                this.Insert(index, s);
        }

        public void Remove(int index)
        {
            if (CurrentIndex > index)
                CurrentIndex--;
            songPaths.RemoveAt(index);
            Song s = outputSongs[index];
            Sorting.RemoveSongFromSortDictionaries(this.outputSongs[index], this.sortDictionaries);
            outputSongs.Remove(this.outputSongs[index]);
        }

        public void Remove(List<int> indexes)
        {
            foreach (int i in indexes)
                Remove(i);
        }

        public void Remove(string path)
        {
            throw new NotImplementedException();
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public void Remove(List<Song> songs)
        {
            foreach (Song s in songs)
                Remove(s.Path);
        }

        public void RemoveAll()
        {
            this.songPaths = new List<string>();
            this.outputSongs = new BindingList<Song>();
            this.CurrentIndex = 0;
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
        }

        public Song GetNext()
        {
            if (songPaths.Count == 0)
                return null;
            if (CurrentIndex + 1 < songPaths.Count)
                return libraryDictionary[songPaths[++CurrentIndex]];
            else
            {
                CurrentIndex = 0;
                return libraryDictionary[songPaths[CurrentIndex]];
            }
        }

        public Song GetPrevious()
        {
            if (songPaths.Count == 0)
                return null;
            if (CurrentIndex > 0)
                return libraryDictionary[songPaths[--CurrentIndex]];
            else
            {
                CurrentIndex = songPaths.Count - 1;
                return libraryDictionary[songPaths[CurrentIndex]];
            }
        }

        public void AddFolder(string path)
        {
            try
            {
                string[] mp3Files = Directory.GetFiles(path, "*" + EXTENSION, SearchOption.AllDirectories);
                foreach (string fileName in mp3Files)
                    songPaths.Add(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add folder to playlist exception: " + ex.ToString());
            }
        }

        public void RemoveFolder(string path)
        {
            throw new NotImplementedException();
        }

        public void Sort(int columnIndex, string field)
        {
            if (this.SortColumnIndex == columnIndex)
            {
                if (this.sortOrder == SortOrder.None || this.sortOrder == SortOrder.Descending)
                    this.sortOrder = SortOrder.Ascending;
                else
                    this.sortOrder = SortOrder.Descending;
            }
            else
                this.sortOrder = SortOrder.Ascending;
            this.SortColumnIndex = columnIndex;
            this.sortField = field;
            Sorting.Sort(field, this.sortOrder, this.sortDictionaries, ref this.outputSongs);
        }

        private void ResetSortVariables()
        {
            this.sortOrder = System.Windows.Forms.SortOrder.None;
            this.SortColumnIndex = -1;
        }

        public BindingList<Song> GetSongs()
        {
            return outputSongs;
        }

        public BindingList<Song> GetAllSongs()
        {
            ResetSortVariables();
            BindingList<Song> outputSongs = GetSongsFromLibrary();
            Sorting.CreateSortDictionaries(outputSongs, this.sortDictionaries);
            return outputSongs;
        }

        public Song Get(string path)
        {
            throw new NotImplementedException();
        }

        public Song GetRandom()
        {
            return libraryDictionary[songPaths[PlayList.r.Next(0, songPaths.Count)]];
        }

        public void Save()
        {
            try
            {
                Stream stream = File.Create(this.Path);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayList));
                serializer.Serialize(stream, this);
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save play list exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Load playlist from file. Returns null if it fails:
        /// </summary>
        /// <param name="path"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static PlayList Load(String path, Library library)
        {
            Stream stream = null;
            PlayList loadedPlayList = null;
            try
            {
                stream = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayList));
                loadedPlayList = (PlayList)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            PlayList pl = new PlayList(library, loadedPlayList.Name, loadedPlayList.songPaths);
            library.LibraryChanged += pl.library_LibraryChanged;
            return pl;
        }
    }
}
