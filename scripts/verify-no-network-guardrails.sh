#!/usr/bin/env bash
set -euo pipefail

manifest="Packages/manifest.json"
if [[ ! -f "$manifest" ]]; then
  echo "Missing $manifest" >&2
  exit 1
fi

package_pattern='analytics|ads|purchasing|iap|services|authentication|cloud|remote-config|crash|firebase|notifications|multiplayer|netcode|relay|lobby'
if rg -n --ignore-case "\"[^\"]*($package_pattern)[^\"]*\"" "$manifest"; then
  echo "Packages/manifest.json contains a package name that needs no-network review." >&2
  exit 1
fi

api_patterns=(
  "UnityWebRequest"
  "System.Net"
  "HttpClient"
  "WebRequest"
  "TcpClient"
  "UdpClient"
  "Socket"
  "Analytics"
  "Advertisement"
  "Purchasing"
  "AuthenticationService"
  "CloudSaveService"
  "RemoteConfigService"
  "CrashReportHandler"
  "Notification"
  "Multiplayer"
)

if [[ -d Assets/Scripts ]]; then
  rg_args=()
  for pattern in "${api_patterns[@]}"; do
    rg_args+=("-e" "$pattern")
  done

  if rg -n --fixed-strings --ignore-case "${rg_args[@]}" Assets/Scripts; then
    echo "Source code contains APIs or SDK terms that need no-network review." >&2
    exit 1
  fi
fi

echo "No banned network SDK packages or source API usage found."
