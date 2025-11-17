using Microsoft.Data.Sqlite;
using TVShowsTransfer.Models;

namespace TVShowsTransfer.Data
{
    public class SQLiteReader
    {
        private readonly string _connectionString;

        public SQLiteReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<DenormalizedTvShow> ReadAll()
        {
            var shows = new List<DenormalizedTvShow>();

            using (var connection = new SqliteConnection(_connectionString))
            {

                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM tv_show_episodes";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    shows.Add(new DenormalizedTvShow
                    {
                        TvShowName = reader.GetString(0),
                        TvShowYear = reader.GetInt32(1),
                        EpisodeName = reader.GetString(2),
                        EpisodeSeason = reader.GetInt32(3),
                        EpisodeNumber = reader.GetInt32(4),
                        CharacterName = reader.GetString(5),
                        ActorName = reader.GetString(6),
                    });
                }
            }

            return shows;
        }
    }
}