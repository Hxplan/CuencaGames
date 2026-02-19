<?php
// ════════════════════════════════════════════════════════════
//  CONFIGURATION BDD (Hostinger)
// ════════════════════════════════════════════════════════════
$host = "localhost"; // Souvent localhost sur Hostinger
$dbname = "u943054996_jeux_multi"; // ⚠️ REMPLACER PAR VOTRE NOM DE BASE
$user = "u943054996_cuenca";          // ⚠️ REMPLACER PAR VOTRE UTILISATEUR
$pass = "MathieuCuenca34410APP";  // ⚠️ REMPLACER PAR VOTRE MOT DE PASSE

header('Content-Type: application/json');

try {
    $pdo = new PDO("mysql:host=$host;dbname=$dbname;charset=utf8", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    die(json_encode(["error" => "Erreur connexion BDD: " . $e->getMessage()]));
}

// Récupération des paramètres (GET ou POST JSON)
$action = $_GET['action'] ?? '';

// Lecture du body JSON pour POST
$input = json_decode(file_get_contents('php://input'), true);

// Si on accède à la page depuis un navigateur sans paramètre, afficher une petite page HTML
if (empty($action) && strpos($_SERVER['HTTP_ACCEPT'] ?? '', 'text/html') !== false) {
        header('Content-Type: text/html; charset=utf-8');
        echo <<<HTML
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <title>API Lobby - Jeux_Multiples</title>
    <style>body{font-family:Arial,Helvetica,sans-serif;margin:24px;color:#222}code{background:#f4f4f4;padding:2px 6px;border-radius:4px}</style>
</head>
<body>
    <h1>API Lobby</h1>
    <p>Cette URL sert d'API pour enregistrer/lister/supprimer des serveurs de jeu.</p>
    <h2>Actions disponibles</h2>
    <ul>
        <li><b>host</b> (POST JSON) : enregistrer un serveur. Champs: <code>name</code>, <code>ip</code>, <code>port</code>.</li>
        <li><b>list</b> (GET) : récupérer la liste des serveurs actifs.</li>
        <li><b>remove</b> (POST JSON) : supprimer un serveur. Champs: <code>ip</code>, <code>port</code>.</li>
        <li><b>lb_record_win</b> (POST JSON) : enregistrer une victoire. Champs: <code>pseudo</code>, <code>game_type</code>.</li>
        <li><b>lb_record_snake</b> (POST JSON) : enregistrer un score Snake (meilleur). Champs: <code>pseudo</code>, <code>score</code>.</li>
        <li><b>lb_list</b> (GET) : récupérer le classement. Param optionnel: <code>game_type</code>.</li>
    </ul>
    <h2>Exemples</h2>
    <p>Lister :</p>
    <pre>curl "https://cuencamathieu.com/lobby_api.php?action=list"</pre>
    <p>Héberger (POST JSON) :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=host" -H "Content-Type: application/json" -d '{"name":"MonServeur","ip":"1.2.3.4","port":8080}'</pre>
    <p>Supprimer :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=remove" -H "Content-Type: application/json" -d '{"ip":"1.2.3.4","port":8080}'</pre>

    <p>Leaderboard (victoire) :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=lb_record_win" -H "Content-Type: application/json" -d '{"pseudo":"Alice","game_type":"Puissance4"}'</pre>
    <p>Leaderboard (Snake score) :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=lb_record_snake" -H "Content-Type: application/json" -d '{"pseudo":"Alice","score":1230}'</pre>
    <p>Leaderboard (liste) :</p>
    <pre>curl "https://cuencamathieu.com/lobby_api.php?action=lb_list"</pre>

    <h2>Diagnostic</h2>
    <p>Si l'API renvoie <code>{"error":"Action inconnue"}</code>, vérifiez l'URL <code>?action=...</code>.</p>
</body>
</html>
HTML;
        exit;
}

// ════════════════════════════════════════════════════════
//  LEADERBOARD HELPERS
// ════════════════════════════════════════════════════════
function ensure_leaderboard_table($pdo) {
    try {
        $pdo->query("CREATE TABLE IF NOT EXISTS leaderboard_stats (
            pseudo     VARCHAR(32) NOT NULL,
            game_type  VARCHAR(32) NOT NULL,
            wins       INT NOT NULL DEFAULT 0,
            best_score INT NOT NULL DEFAULT 0,
            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (pseudo, game_type),
            KEY idx_game_type (game_type),
            KEY idx_wins (wins),
            KEY idx_best_score (best_score)
        )");
    } catch (Exception $e) { /* ignore */ }
}

if ($action === 'host') {
    // ════════════════════════════════════════════════════════
    //  ACTION: HOST (Enregistrer un serveur)
    // ════════════════════════════════════════════════════════
    if (!isset($input['name'], $input['ip'], $input['port'])) {
        echo json_encode(["error" => "Donnees manquantes (name, ip, port)"]);
        exit;
    }

    $name = substr(strip_tags($input['name']), 0, 50); // Sécurisation basique HTML
    $ip = filter_var($input['ip'], FILTER_VALIDATE_IP);
    $local_ip = isset($input['local_ip']) ? filter_var($input['local_ip'], FILTER_VALIDATE_IP) : null;
    $port = intval($input['port']);
    $game_type = isset($input['game_type']) ? substr(trim(strip_tags($input['game_type'])), 0, 32) : 'any';
    $host_pseudo = isset($input['host_pseudo']) ? substr(trim(strip_tags($input['host_pseudo'])), 0, 64) : $name;

    if (!$ip || $port <= 0 || $port > 65535) {
        echo json_encode(["error" => "IP ou Port invalide"]);
        exit;
    }

    // On supprime d'abord si la même IP/Port existe déjà (éviter doublons)
    $stmt = $pdo->prepare("DELETE FROM game_servers WHERE ip_address = ? AND port = ?");
    $stmt->execute([$ip, $port]);

    // Ensure optional columns exist (best-effort)
    foreach (['local_ip' => 'VARCHAR(45) NULL', 'game_type' => 'VARCHAR(32) NOT NULL DEFAULT \'any\'', 'host_pseudo' => 'VARCHAR(64) NULL'] as $col => $def) {
        try {
            $colCheck = $pdo->query("SHOW COLUMNS FROM game_servers LIKE '$col'")->fetch();
            if (!$colCheck) {
                $pdo->query("ALTER TABLE game_servers ADD COLUMN $col $def");
            }
        } catch (Exception $e) { /* ignore */ }
    }

    // Insertion (server_name = nom du salon, host_pseudo = pseudo hôte, game_type)
    $stmt = $pdo->prepare("INSERT INTO game_servers (server_name, ip_address, local_ip, port, game_type, host_pseudo) VALUES (?, ?, ?, ?, ?, ?)");
    $stmt->execute([$name, $ip, $local_ip ?: null, $port, $game_type, $host_pseudo]);

    echo json_encode(["success" => true, "id" => $pdo->lastInsertId()]);

} elseif ($action === 'list') {
    // ════════════════════════════════════════════════════════
    //  ACTION: LIST (Lister les serveurs actifs)
    // ════════════════════════════════════════════════════════
    
    // 1. Nettoyage automatique : Supprimer les serveurs vieux de > 2 minutes
    $pdo->query("DELETE FROM game_servers WHERE last_ping < (NOW() - INTERVAL 2 MINUTE)");

    // 2. Récupérer la liste (game_type et host_pseudo pour affichage)
    try {
        $stmt = $pdo->query("SELECT id, server_name, ip_address, COALESCE(local_ip, '') AS local_ip, port, COALESCE(game_type, 'any') AS game_type, COALESCE(host_pseudo, server_name) AS host_pseudo FROM game_servers ORDER BY created_at DESC LIMIT 50");
        $servers = $stmt->fetchAll(PDO::FETCH_ASSOC);
    } catch (Exception $e) {
        // Ancienne BDD sans game_type/host_pseudo
        $stmt = $pdo->query("SELECT id, server_name, ip_address, COALESCE(local_ip, '') AS local_ip, port FROM game_servers ORDER BY created_at DESC LIMIT 50");
        $servers = $stmt->fetchAll(PDO::FETCH_ASSOC);
        foreach ($servers as &$row) {
            $row['game_type']   = 'any';
            $row['host_pseudo'] = isset($row['server_name']) ? $row['server_name'] : '?';
        }
        unset($row);
    }

    echo json_encode($servers);

} elseif ($action === 'remove') {
    // ════════════════════════════════════════════════════════
    //  ACTION: REMOVE (Quand l'hôte ferme le jeu)
    // ════════════════════════════════════════════════════════
    if (!isset($input['ip'], $input['port'])) {
        echo json_encode(["error" => "IP/Port manquants"]);
        exit;
    }

    $ip = $input['ip'];
    $port = intval($input['port']);

    $stmt = $pdo->prepare("DELETE FROM game_servers WHERE ip_address = ? AND port = ?");
    $stmt->execute([$ip, $port]);

    echo json_encode(["success" => true]);

} elseif ($action === 'lb_record_win') {
    // ════════════════════════════════════════════════════════
    //  LEADERBOARD: RECORD WIN
    // ════════════════════════════════════════════════════════
    ensure_leaderboard_table($pdo);
    if (!isset($input['pseudo'], $input['game_type'])) {
        echo json_encode(["error" => "Donnees manquantes (pseudo, game_type)"]);
        exit;
    }
    $pseudo = substr(trim(strip_tags($input['pseudo'])), 0, 32);
    $game   = substr(trim(strip_tags($input['game_type'])), 0, 32);
    if ($pseudo === "" || $game === "") { echo json_encode(["error" => "Pseudo/Jeu invalide"]); exit; }

    $stmt = $pdo->prepare("
        INSERT INTO leaderboard_stats (pseudo, game_type, wins, best_score)
        VALUES (?, ?, 1, 0)
        ON DUPLICATE KEY UPDATE wins = wins + 1
    ");
    $stmt->execute([$pseudo, $game]);
    echo json_encode(["success" => true]);

} elseif ($action === 'lb_record_snake') {
    // ════════════════════════════════════════════════════════
    //  LEADERBOARD: RECORD SNAKE BEST SCORE
    // ════════════════════════════════════════════════════════
    ensure_leaderboard_table($pdo);
    if (!isset($input['pseudo'], $input['score'])) {
        echo json_encode(["error" => "Donnees manquantes (pseudo, score)"]);
        exit;
    }
    $pseudo = substr(trim(strip_tags($input['pseudo'])), 0, 32);
    $score  = intval($input['score']);
    if ($pseudo === "" || $score <= 0) { echo json_encode(["error" => "Pseudo/Score invalide"]); exit; }

    $stmt = $pdo->prepare("
        INSERT INTO leaderboard_stats (pseudo, game_type, wins, best_score)
        VALUES (?, 'Snake', 0, ?)
        ON DUPLICATE KEY UPDATE best_score = GREATEST(best_score, VALUES(best_score))
    ");
    $stmt->execute([$pseudo, $score]);
    echo json_encode(["success" => true]);

} elseif ($action === 'lb_list') {
    // ════════════════════════════════════════════════════════
    //  LEADERBOARD: LIST
    // ════════════════════════════════════════════════════════
    ensure_leaderboard_table($pdo);
    $game = $_GET['game_type'] ?? '';
    if ($game) {
        $game = substr(trim(strip_tags($game)), 0, 32);
        if ($game === "Snake") {
            $stmt = $pdo->prepare("SELECT pseudo, best_score AS value FROM leaderboard_stats WHERE game_type='Snake' ORDER BY best_score DESC, updated_at DESC LIMIT 50");
            $stmt->execute();
        } else {
            $stmt = $pdo->prepare("SELECT pseudo, wins AS value FROM leaderboard_stats WHERE game_type=? ORDER BY wins DESC, updated_at DESC LIMIT 50");
            $stmt->execute([$game]);
        }
        echo json_encode(["game_type" => $game, "items" => $stmt->fetchAll(PDO::FETCH_ASSOC)]);
        exit;
    }

    // Global: top 10 par jeu
    $games = ["Puissance4", "Dames", "MortPion", "BlackJack", "Poker", "Snake"];
    $out = [];
    foreach ($games as $g) {
        if ($g === "Snake") {
            $stmt = $pdo->query("SELECT pseudo, best_score AS value FROM leaderboard_stats WHERE game_type='Snake' ORDER BY best_score DESC, updated_at DESC LIMIT 10");
        } else {
            $stmt = $pdo->prepare("SELECT pseudo, wins AS value FROM leaderboard_stats WHERE game_type=? ORDER BY wins DESC, updated_at DESC LIMIT 10");
            $stmt->execute([$g]);
        }
        $out[$g] = $stmt->fetchAll(PDO::FETCH_ASSOC);
    }
    echo json_encode($out);

} else {
    echo json_encode(["error" => "Action inconnue"]);
}
?>
