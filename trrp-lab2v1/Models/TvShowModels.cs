namespace TVShowsTransfer.Models
{
    public class DenormalizedTvShow
    {
        public string TvShowName { get; set; } = "";
        public int TvShowYear { get; set; }
        public string EpisodeName { get; set; } = "";
        public int EpisodeSeason { get; set; }
        public int EpisodeNumber { get; set; }
        public string CharacterName { get; set; } = "";
        public string ActorName { get; set; } = "";
    }

    public class DataNormalizer
    {
        public List<TvShow> TvShows { get; } = new();
        public List<Episode> Episodes { get; } = new();
        public List<Actor> Actors { get; } = new();
        public List<Character> Characters { get; } = new();
        public List<M2MEpisodeCharacter> M2MEpisodeCharacters { get; } = new();

        private Dictionary<(string, int), int> _tvShowKeyToId = [];
        private Dictionary<(string, int, int), int> _episodeKeyToId = [];
        private Dictionary<string, int> _actorKeyToId = [];
        private Dictionary<string, int> _characterKeyToId = [];

        private int _currentTvShowId = 1;
        private int _currentEpisodeId = 1;
        private int _currentActorId = 1;
        private int _currentCharacterId = 1;

        public void NormalizeData(List<DenormalizedTvShow> denormalizedTvShows)
        {
            foreach (var denormTvShow in denormalizedTvShows)
            {
                var tvShowKey = (denormTvShow.TvShowName, denormTvShow.TvShowYear);
                if (!_tvShowKeyToId.TryGetValue(tvShowKey, out int tvShowId))
                {
                    var newTvShow = new TvShow
                    {
                        Id = _currentTvShowId,
                        Name = denormTvShow.TvShowName,
                        Year = denormTvShow.TvShowYear
                    };
                    TvShows.Add(newTvShow);
                    _tvShowKeyToId.Add(tvShowKey, _currentTvShowId);
                    tvShowId = _currentTvShowId;
                    _currentTvShowId++;
                }

                var episodeKey = (denormTvShow.EpisodeName, denormTvShow.EpisodeSeason, denormTvShow.EpisodeNumber);
                if (!_episodeKeyToId.TryGetValue(episodeKey, out int episodeId))
                {
                    var newEpisode = new Episode
                    {
                        Id = _currentEpisodeId,
                        Name = denormTvShow.EpisodeName,
                        Season = denormTvShow.EpisodeSeason,
                        Number = denormTvShow.EpisodeNumber,
                        TvShowId = tvShowId
                    };
                    Episodes.Add(newEpisode);
                    _episodeKeyToId.Add(episodeKey, _currentEpisodeId);
                    episodeId = _currentEpisodeId;
                    _currentEpisodeId++;
                }

                var actorKey = denormTvShow.ActorName;
                if (!_actorKeyToId.TryGetValue(actorKey, out int actorId))
                {
                    var newActor = new Actor
                    {
                        Id = _currentActorId,
                        Name = denormTvShow.ActorName
                    };
                    Actors.Add(newActor);
                    _actorKeyToId.Add(actorKey, _currentActorId);
                    actorId = _currentActorId;
                    _currentActorId++;
                }

                var characterKey = denormTvShow.CharacterName;
                if (!_characterKeyToId.TryGetValue(characterKey, out int characterId))
                {
                    var newCharacter = new Character
                    {
                        Id = _currentCharacterId,
                        Name = denormTvShow.CharacterName,
                        ActorId = actorId
                    };
                    Characters.Add(newCharacter);
                    _characterKeyToId.Add(characterKey, _currentCharacterId);
                    characterId = _currentCharacterId;
                    _currentCharacterId++;
                }

                bool existingM2mCharacterEpisodeExists = M2MEpisodeCharacters.Any(m2m => m2m.EpisodeId == episodeId && m2m.CharacterId == characterId);
                if (!existingM2mCharacterEpisodeExists)
                {
                    var newM2MCharacterEpisode = new M2MEpisodeCharacter
                    {
                        CharacterId = characterId,
                        EpisodeId = episodeId
                    };
                    M2MEpisodeCharacters.Add(newM2MCharacterEpisode);
                }
            }
        }
    }

    public class TvShow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Year { get; set; }
    }

    public class Episode
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Season { get; set; }
        public int Number { get; set; }
        public int TvShowId { get; set; }
    }

    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int ActorId { get; set; }
    }

    public class M2MEpisodeCharacter
    {
        public int EpisodeId { get; set; }
        public int CharacterId { get; set; }
    }
}