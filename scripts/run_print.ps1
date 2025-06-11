$containerName = "forwardly-db"
$localScriptPath = "scripts/print_all_tables.sh"
$containerScriptPath = "/tmp/print_all_tables.sh"

docker cp $localScriptPath "${containerName}:${containerScriptPath}"

docker exec $containerName bash -c ${containerScriptPath}
