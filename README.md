# Noord-Hollands Archief pre-ingest workerservice

Een .NET (Core) gebaseerde workerservice met het doel om een set REST API call automatisch te verwerken volgens een werkschema. De workerservice werkt samen met de preingest REST API service. De workerservice maakt een verbinding, via een websocket (SignalR), met de preingest REST API service voor real-time terugkoppeling.

## Docker
De workerservice wordt als een Docker image gecompileerd. Om de image te kunnen gebruiken is een Docker omgeving nodig met ondersteuning voor Linux. Voor Windows besturingsystemen is Hyper-V nodig of WSL2.

De workerservice werkt samen met de preingest REST API service en het maakt een verbinding met websocket `http://preingest-api/preingestEventHub/`.

## Trademarks
Preservica™ is een handelsmerk van [Preservica Ltd](https://preservica.com/). Noord-Hollands Archief is niet aangesloten aan Preservica™. 
