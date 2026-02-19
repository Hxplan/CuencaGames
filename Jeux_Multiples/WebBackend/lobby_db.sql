CREATE TABLE IF NOT EXISTS game_servers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    server_name VARCHAR(50) NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    port INT NOT NULL,
    game_type VARCHAR(32) NOT NULL DEFAULT 'any',
    host_pseudo VARCHAR(64) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_ping TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ════════════════════════════════════════════════════════════
--  LEADERBOARD (en ligne)
--  - wins: jeux multi (Puissance4, Dames, MortPion, BlackJack, Poker)
--  - best_score: Snake (solo)
-- ════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS leaderboard_stats (
    pseudo     VARCHAR(32) NOT NULL,
    game_type  VARCHAR(32) NOT NULL,
    wins       INT NOT NULL DEFAULT 0,
    best_score INT NOT NULL DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (pseudo, game_type),
    KEY idx_game_type (game_type),
    KEY idx_wins (wins),
    KEY idx_best_score (best_score)
);
