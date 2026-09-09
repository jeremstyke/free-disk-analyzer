# Free Disk Analyzer

[![License: All Rights Reserved](https://img.shields.io/badge/License-All%20Rights%20Reserved-lightgrey.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D6)](#)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4)](#)
[![Build](https://img.shields.io/github/actions/workflow/status/jeremstyke/free-disk-analyzer/build.yml?branch=main)](../../actions)
[![Release](https://img.shields.io/github/v/release/jeremstyke/free-disk-analyzer)](../../releases/latest)

**Un analyseur d'espace disque gratuit et respectueux de la vie privée pour Windows.**
Découvrez ce qui occupe réellement votre stockage, avec une interface moderne et facile à utiliser.

🇬🇧 [Read in English](README.md)

<p align="center">
  <a href="../../releases/latest/download/FreeDiskAnalyzer-Setup.exe">
    <img src="https://img.shields.io/badge/%E2%86%93%20T%C3%A9l%C3%A9charger-pour%20Windows-2563EB?style=for-the-badge" alt="Télécharger pour Windows" />
  </a>
</p>

> [Voir toutes les versions](../../releases)

---

## Fonctionnalités

- Scan disque rapide, jamais bloquant (async, annulable à tout moment)
- Tableau de bord avec vue d'ensemble des disques, graphique donut utilisé/libre, répartition du stockage par catégorie, résumé du dernier scan
- Vues Largest Folders et Largest Files
- Recherche dédiée de fichiers volumineux avec filtres de taille (100 Mo, 500 Mo, 1 Go, 5 Go, personnalisé)
- Vue hiérarchique des dossiers pour repérer rapidement ce qui consomme l'espace
- Détecteur de fichiers en double (1 Mo et plus, comparés par contenu, pas juste nom/taille)
- Détecteur de fichiers anciens, ceux probablement oubliés, triés par date de modification
- Détecteur de dossiers vides
- Export d'un rapport de scan en CSV
- Recommandation NordVPN sur le tableau de bord, et recommandation DeleteMe après un scan, toutes deux clairement identifiées comme liens affiliés
- Paramètres : langue, thème clair/sombre, démarrage avec Windows, statistiques anonymes en opt-in, réinitialisation, tout sauvegardé localement
- Page Confidentialité dédiée expliquant précisément ce qui reste local
- Mode clair et sombre, interface moderne inspirée de Windows 11
- Anglais et français, couverture complète (sélecteur de langue dans Paramètres, redémarrage requis pour appliquer)
- Analyse 100 % locale. Rien concernant vos fichiers ou dossiers n'est jamais envoyé
- Gratuit pour toujours. Pas d'abonnement, pas de version premium, pas de limite artificielle

## Installation

1. Rendez-vous sur la [dernière version](../../releases/latest)
2. Téléchargez `FreeDiskAnalyzer-Setup.exe` (installateur) ou `FreeDiskAnalyzer-Portable.zip` (portable)
3. Lancez l'application. Windows peut afficher un avertissement SmartScreen, voir ci-dessous.

### À propos de l'avertissement de sécurité Windows

Free Disk Analyzer est actuellement distribué sans certificat commercial de signature de code. Windows SmartScreen peut donc afficher un avertissement car l'éditeur de l'application ne peut pas encore être vérifié.

Cela ne signifie pas que Free Disk Analyzer est un logiciel malveillant. Vous pouvez vérifier le fichier téléchargé à l'aide de l'empreinte SHA-256 publiée avec chaque version (`SHA256SUMS.txt`).

## Confidentialité

Free forever. Privacy first.

- L'analyse du disque est 100 % locale. Noms de fichiers, chemins, contenus et structure de dossiers ne sont jamais envoyés.
- L'application fonctionne entièrement hors ligne.
- Des statistiques d'usage anonymes et minimales peuvent être activées dans les paramètres (installations, lancements, nombre de scans, version de l'application, version de Windows, pays approximatif). Aucune donnée personnelle, aucun fingerprinting, aucun tracking caché.

Détails complets : [PRIVACY.md](PRIVACY.md)

## Divulgation d'affiliation

Certains liens dans Free Disk Analyzer (tableau de bord et après un scan) sont des liens affiliés (NordVPN, DeleteMe). Si vous achetez via l'un de ces liens, nous pouvons percevoir une commission sans surcoût pour vous. Ces commissions aident à financer le développement.

Détails complets : [AFFILIATE-DISCLOSURE.md](AFFILIATE-DISCLOSURE.md)

## Architecture

```
Free-Disk-Analyzer/
├── src/
│   ├── FreeDiskAnalyzer/          # Application WPF (UI, vues, view models)
│   └── FreeDiskAnalyzer.Core/     # Moteur de scan, modèles, services (sans dépendance UI)
├── tests/
│   └── FreeDiskAnalyzer.Tests/    # Tests unitaires
├── assets/                        # Icônes, logos, images
├── docs/                          # Documentation complémentaire
├── website/                       # Site GitHub Pages (publié une fois le dépôt public)
├── installer/                     # Script Inno Setup produisant FreeDiskAnalyzer-Setup.exe
└── .github/                       # Workflows, templates d'issues et de PR
```

## Développement

Prérequis :
- Windows 10/11 x64
- .NET 8 SDK
- Visual Studio 2022 (ou `dotnet build` en ligne de commande)

```bash
git clone https://github.com/jeremstyke/free-disk-analyzer.git
cd free-disk-analyzer
dotnet build
```

## Contribuer

Ce projet n'accepte pas de pull requests externes. Les rapports de bugs et demandes de fonctionnalités via les [issues](../../issues) sont les bienvenus, voir [CONTRIBUTING.md](CONTRIBUTING.md) et [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Sécurité

Voir [SECURITY.md](SECURITY.md) pour signaler une vulnérabilité.

## Licence

Tous droits réservés. L'application est gratuite à l'usage, le code source n'est pas libre de réutilisation ou de redistribution. Voir [LICENSE](LICENSE).

## Visibilité du dépôt

Ce dépôt est actuellement privé pendant le développement. Il sera rendu public (ou restructuré) une fois l'application dans un état fonctionnel et téléchargeable. En attendant, les releases et GitHub Pages ne sont pas accessibles publiquement, voir [ROADMAP.md](docs/ROADMAP.md).

## Autres projets du développeur

[CleanTab - Browser Cleaner](https://getcleantab.com/) : un nettoyeur de navigateur gratuit et respectueux de la vie privée pour Chrome et Edge, même philosophie "gratuit pour toujours, confidentialité d'abord" que ce projet, par le même développeur.
