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
  <a href="https://jeremstyke.github.io/free-disk-analyzer/fr/">
    <img src="https://img.shields.io/badge/%F0%9F%8C%90%20Site%20web-2563EB?style=for-the-badge" alt="Site web" />
  </a>
</p>

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
- Détecteur de fichiers en double (1 Mo et plus, comparés par contenu, pas juste nom/taille), suppression des copies en trop directement depuis la liste (garde toujours au moins une copie, envoie à la Corbeille)
- Détecteur de fichiers anciens, ceux probablement oubliés, triés par date de modification
- Détecteur de dossiers vides, avec suppression en un clic (Corbeille, pas définitif)
- Nettoyage des navigateurs : efface le cache, les cookies et l'historique pour Chrome, Edge et Firefox (favoris et mots de passe jamais touchés, historique Firefox volontairement exclu car mélangé aux favoris dans le même fichier)
- Libération de RAM (vrais chiffres avant/après, pas un gain promis)
- Notifications de mise à jour dans l'app, avec téléchargement et installation en un clic
- Derniers articles du blog affichés sur le tableau de bord
- Nettoyage PC : fichiers temporaires et Corbeille (onglet Système dans Nettoyage)
- Liste blanche de cookies (onglet Navigateurs) : reste connecté sur certains sites lors du nettoyage
- Indicateurs de risque vert/orange/rouge sur chaque fonctionnalité de suppression
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

Cela ne signifie pas que Free Disk Analyzer est un logiciel malveillant. Le code source est consultable publiquement sur GitHub. Vous pouvez aussi vérifier le fichier téléchargé à l'aide de l'empreinte SHA-256 publiée avec chaque version (`SHA256SUMS.txt`).

## Confidentialité

Free forever. Privacy first.

- L'analyse du disque est 100 % locale. Noms de fichiers, chemins, contenus et structure de dossiers ne sont jamais envoyés.
- L'application fonctionne entièrement hors ligne.
- Des statistiques d'usage anonymes et minimales peuvent être activées dans les paramètres (installations, lancements, nombre de scans, version de l'application, version de Windows, pays approximatif). Aucune donnée personnelle, aucun fingerprinting, aucun tracking caché.

Détails complets : [PRIVACY.md](PRIVACY.md)

## Divulgation d'affiliation

Certains liens dans Free Disk Analyzer (tableau de bord et après un scan) sont des liens affiliés :

- [NordVPN](https://go.nordvpn.net/aff_c?offer_id=15&aff_id=155375&source=Free%20disk%20analyzer)
- [DeleteMe](https://www.de33watrk.com/WCKMXS/KMKS9/)

Si vous achetez via l'un de ces liens, nous pouvons percevoir une commission sans surcoût pour vous. Ces commissions aident à financer le développement.

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

Ce dépôt est public.

## Soutien

Si Free Disk Analyzer vous est utile, vous pouvez [offrir un café](https://jeremstyke.gumroad.com/coffee).
