<?php
// ════════════════════════════════════════════════════════════
//  CONFIGURATION BDD (Hostinger)
// ════════════════════════════════════════════════════════════
$host = "localhost"; // Souvent localhost sur Hostinger
$dbname = "u123456789_masuperbase"; // ⚠️ REMPLACER PAR VOTRE NOM DE BASE
$user = "u123456789_user";          // ⚠️ REMPLACER PAR VOTRE UTILISATEUR
$pass = "MonMotDePasseSecure123!";  // ⚠️ REMPLACER PAR VOTRE MOT DE PASSE

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
    $port = intval($input['port']);

    if (!$ip || $port <= 0 || $port > 65535) {
        echo json_encode(["error" => "IP ou Port invalide"]);
        exit;
    }

    // On supprime d'abord si la même IP/Port existe déjà (éviter doublons)
    $stmt = $pdo->prepare("DELETE FROM game_servers WHERE ip_address = ? AND port = ?");
    $stmt->execute([$ip, $port]);

    // Insertion
    $stmt = $pdo->prepare("INSERT INTO game_servers (server_name, ip_address, port) VALUES (?, ?, ?)");
    $stmt->execute([$name, $ip, $port]);

    echo json_encode(["success" => true, "id" => $pdo->lastInsertId()]);

} elseif ($action === 'list') {
    // ════════════════════════════════════════════════════════
    //  ACTION: LIST (Lister les serveurs actifs)
    // ════════════════════════════════════════════════════════
    
    // 1. Nettoyage automatique : Supprimer les serveurs vieux de > 2 minutes
    // (L'application C# devra "pinger" ou recréer l'entrée régulièrement, ou juste suppression au timeout)
    $pdo->query("DELETE FROM game_servers WHERE last_ping < (NOW() - INTERVAL 2 MINUTE)");

    // 2. Récupérer la liste
    $stmt = $pdo->query("SELECT id, server_name, ip_address, port FROM game_servers ORDER BY created_at DESC LIMIT 50");
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
