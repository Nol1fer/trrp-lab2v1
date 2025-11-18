using Npgsql;
using TVShowsTransfer.Models;

namespace TVShowsTransfer.Data
{
    public class PostgreSQLWriter
    {
        private readonly string _connectionString;

        public PostgreSQLWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task DropAndCreateTables()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                DROP TABLE IF EXISTS m2m_episode_character CASCADE;          
                DROP TABLE IF EXISTS episode CASCADE;          
                DROP TABLE IF EXISTS tv_show CASCADE;          
                DROP TABLE IF EXISTS character CASCADE;          
                DROP TABLE IF EXISTS actor CASCADE;          
            ";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Dropped existing tables (if any).");

                cmd.CommandText = @"
                CREATE TABLE tv_show (
                    t_id INTEGER PRIMARY KEY,
                    t_name VARCHAR(200),
                    t_year INTEGER
                );

                CREATE TABLE episode (
                    e_id INTEGER PRIMARY KEY,
                    e_name VARCHAR(200),
                    e_season INTEGER,
                    e_number INTEGER,
                    e_tv_show INTEGER,
                    CONSTRAINT fk_tv_show
                        FOREIGN KEY(e_tv_show) 
                        REFERENCES tv_show(t_id)
                );

                CREATE TABLE actor (
                    a_id INTEGER PRIMARY KEY,
                    a_name VARCHAR(200)
                );
   
                CREATE TABLE character (
                    c_id INTEGER PRIMARY KEY,
                    c_name VARCHAR(200),
                    c_actor INTEGER,
                    CONSTRAINT fk_actor
                        FOREIGN KEY(c_actor) 
                        REFERENCES actor(a_id)
                );

                CREATE TABLE m2m_episode_character (
                    mec_e_id INTEGER NOT NULL,
                    mec_c_id INTEGER NOT NULL,
	                CONSTRAINT pk_m2m 
		                PRIMARY KEY(mec_e_id, mec_c_id),
                    CONSTRAINT fk_episode
                        FOREIGN KEY(mec_e_id) 
                        REFERENCES episode(e_id),
                    CONSTRAINT fk_character
                        FOREIGN KEY(mec_c_id) 
                        REFERENCES character(c_id),
	                CONSTRAINT unique_m2m
		                UNIQUE(mec_e_id, mec_c_id)
                );
            ";
                cmd.ExecuteNonQuery();
                await transaction.CommitAsync();

                Console.WriteLine("Created tables.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Save failed: {ex.Message}");
                throw;
            }
        }

        public async Task SaveNormalizedDataAsync(DataNormalizer normalizer)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var cmd = connection.CreateCommand();

                foreach (var tvShow in normalizer.TvShows)
                {
                    cmd = new NpgsqlCommand("INSERT INTO tv_show (t_id, t_name, t_year) VALUES (@t_id, @t_name, @t_year)", connection);
                    cmd.Parameters.AddWithValue("t_id", tvShow.Id);
                    cmd.Parameters.AddWithValue("t_name", tvShow.Name);
                    cmd.Parameters.AddWithValue("t_year", tvShow.Year);
                    cmd.ExecuteNonQuery();
                }

                foreach (var episode in normalizer.Episodes)
                {
                    cmd = new NpgsqlCommand("INSERT INTO episode (e_id, e_name, e_season, e_number, e_tv_show) VALUES (@e_id, @e_name, @e_season, @e_number, @e_tv_show)", connection);
                    cmd.Parameters.AddWithValue("e_id", episode.Id);
                    cmd.Parameters.AddWithValue("e_name", episode.Name);
                    cmd.Parameters.AddWithValue("e_season", episode.Season);
                    cmd.Parameters.AddWithValue("e_number", episode.Number);
                    cmd.Parameters.AddWithValue("e_tv_show", episode.TvShowId);
                    cmd.ExecuteNonQuery();
                }

                foreach (var actor in normalizer.Actors)
                {
                    cmd = new NpgsqlCommand("INSERT INTO actor (a_id, a_name) VALUES (@a_id, @a_name)", connection);
                    cmd.Parameters.AddWithValue("a_id", actor.Id);
                    cmd.Parameters.AddWithValue("a_name", actor.Name);
                    cmd.ExecuteNonQuery();
                }

                foreach (var character in normalizer.Characters)
                {
                    cmd = new NpgsqlCommand("INSERT INTO character (c_id, c_name, c_actor) VALUES (@c_id, @c_name, @c_actor)", connection);
                    cmd.Parameters.AddWithValue("c_id", character.Id);
                    cmd.Parameters.AddWithValue("c_name", character.Name);
                    cmd.Parameters.AddWithValue("c_actor", character.ActorId);
                    cmd.ExecuteNonQuery();
                }

                foreach (var m2mEpisodeCharacter in normalizer.M2MEpisodeCharacters)
                {
                    cmd = new NpgsqlCommand("INSERT INTO m2m_episode_character (mec_e_id, mec_c_id) VALUES (@mec_e_id, @mec_c_id)", connection);
                    cmd.Parameters.AddWithValue("mec_e_id", m2mEpisodeCharacter.EpisodeId);
                    cmd.Parameters.AddWithValue("mec_c_id", m2mEpisodeCharacter.CharacterId);
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Inserted data.");

                await transaction.CommitAsync();
                Console.WriteLine("Data saved to PostgreSQL");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Save failed: {ex.Message}");
                throw;
            }
        }
    }
}