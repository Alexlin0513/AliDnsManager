# AliDnsManager · Alibaba Cloud DNS Manager (WPF)

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-informational)](#)
[![UI](https://img.shields.io/badge/UI-WPF-7952B3)](#)
[![License](https://img.shields.io/badge/License-Custom-lightgrey)](#license)

A modern WPF desktop app to manage Alibaba Cloud DNS records.

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Screenshots](#screenshots)
- [License](#license)
- [Related Docs](#related-docs)

## Introduction
A desktop app that connects to the AliDNS API, lets you list domains and manage DNS records.

## Features
- Connect to AliDNS API; list domains; manage DNS records (CRUD); enable/disable
- Supported record types: A, AAAA, CNAME, MX, TXT, NS, SRV, CAA
- Secure local config storage (Windows DPAPI)
- Modern WPF UI

## Requirements
- Windows 10/11
- .NET 9.0 Runtime

## Quick Start
1. Prepare a RAM user AccessKey with least privilege
2. Launch the app, enter AccessKeyId/AccessKeySecret, click Connect
3. Select a domain and manage records (add/edit/delete/enable/disable)

## Screenshots
- TBA

## License
- For learning and personal use only

## Related Docs
- Deployment: see [deploy.md](./deploy.md)
- Test scope and cases: see repository root test.md

