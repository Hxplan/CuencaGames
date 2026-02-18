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
    </ul>
    <h2>Exemples</h2>
    <p>Lister :</p>
    <pre>curl "https://cuencamathieu.com/lobby_api.php?action=list"</pre>
    <p>Héberger (POST JSON) :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=host" -H "Content-Type: application/json" -d '{"name":"MonServeur","ip":"1.2.3.4","port":8080}'</pre>
    <p>Supprimer :</p>
    <pre>curl -X POST "https://cuencamathieu.com/lobby_api.php?action=remove" -H "Content-Type: application/json" -d '{"ip":"1.2.3.4","port":8080}'</pre>

    <h2>Diagnostic</h2>
    <p>Si l'API renvoie <code>{"error":"Action inconnue (host, list, remove)"}</code>, vérifiez que vous appelez avec <code>?action=host</code>, <code>?action=list</code> ou <code>?action=remove</code>.</p>
</body>
</html>
HTML;
        exit;
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

    if (!$ip || $port <= 0 || $port > 65535) {
        echo json_encode(["error" => "IP ou Port invalide"]);
        exit;
    }

    // On supprime d'abord si la même IP/Port existe déjà (éviter doublons)
    $stmt = $pdo->prepare("DELETE FROM game_servers WHERE ip_address = ? AND port = ?");
    $stmt->execute([$ip, $port]);

    // Ensure local_ip column exists (best-effort)
    try {
        $col = $pdo->query("SHOW COLUMNS FROM game_servers LIKE 'local_ip'")->fetch();
        if (!$col) {
            $pdo->query("ALTER TABLE game_servers ADD COLUMN local_ip VARCHAR(45) NULL");
        }
    } catch (Exception $e) { /* ignore if cannot alter */ }

    // Insertion (incluant local_ip si fourni)
    if ($local_ip) {
        $stmt = $pdo->prepare("INSERT INTO game_servers (server_name, ip_address, local_ip, port) VALUES (?, ?, ?, ?)");
        $stmt->execute([$name, $ip, $local_ip, $port]);
    } else {
        $stmt = $pdo->prepare("INSERT INTO game_servers (server_name, ip_address, port) VALUES (?, ?, ?)");
        $stmt->execute([$name, $ip, $port]);
    }

    echo json_encode(["success" => true, "id" => $pdo->lastInsertId()]);

} elseif ($action === 'list') {
    // ════════════════════════════════════════════════════════
    //  ACTION: LIST (Lister les serveurs actifs)
    // ════════════════════════════════════════════════════════
    
    // 1. Nettoyage automatique : Supprimer les serveurs vieux de > 2 minutes
    // (L'application C# devra "pinger" ou recréer l'entrée régulièrement, ou juste suppression au timeout)
    $pdo->query("DELETE FROM game_servers WHERE last_ping < (NOW() - INTERVAL 2 MINUTE)");

    // 2. Récupérer la liste
    $stmt = $pdo->query("SELECT id, server_name, ip_address, COALESCE(local_ip, '') as local_ip, port FROM game_servers ORDER BY created_at DESC LIMIT 50");
    $servers = $stmt->fetchAll(PDO::FETCH_ASSOC);

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

} else {
    echo json_encode(["error" => "Action inconnue (host, list, remove)"]);
}
?>
