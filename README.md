<img align="right" width="100" height="100" src="https://github.com/user-attachments/assets/00fbb6b8-e29e-409c-aac8-67bdece3f6aa">

### MtaServerQuery
###### Librería .NET para consultar datos de servidores **Multi Theft Auto (MTA)** usando UDP.

---

## Instalación

```bash
dotnet add package MtaServerQuery
```

## Uso rápido

```csharp
using MtaServerQuery.Client;

var service = new MtaServerService();

var players = await service.GetPlayersAsync("127.0.0.1", 22003);
var isConnected = await service.IsConnectedAsync("Jugador", "127.0.0.1", 22003);
```

## Funcionalidades

- Obtener lista de jugadores conectados.
- Consultar ping y score por jugador.
- Verificar si un jugador específico está conectado.
