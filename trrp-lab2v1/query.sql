SELECT 
    ts.t_id AS show_id,
    ts.t_name AS show_name,
    ts.t_year AS show_year,
    e.e_id AS episode_id,
    e.e_name AS episode_name,
    e.e_season AS season,
    e.e_number AS episode_number,
    a.a_id AS actor_id,
    a.a_name AS actor_name,
    c.c_id AS character_id,
    c.c_name AS character_name
FROM tv_show ts
INNER JOIN episode e ON ts.t_id = e.e_tv_show
INNER JOIN m2m_episode_character mec ON e.e_id = mec.mec_e_id
INNER JOIN character c ON mec.mec_c_id = c.c_id
INNER JOIN actor a ON c.c_actor = a.a_id
ORDER BY ts.t_name, e.e_season, e.e_number, a.a_name;