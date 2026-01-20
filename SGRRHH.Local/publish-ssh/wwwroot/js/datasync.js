// SignalR client connection helper for data synchronization
// Este script maneja la conexión al Hub de SignalR y notifica a componentes Blazor

window.dataSyncConnection = null;

window.connectDataSync = async function (dotNetHelper) {
    try {
        if (window.dataSyncConnection) {
            console.log('[DataSync] Ya existe una conexión activa');
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/datasync")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("DataChanged", (entityType, changeType, entityId) => {
            console.log(`[DataSync] Cambio recibido: ${entityType} - ${changeType} - ${entityId}`);
            dotNetHelper.invokeMethodAsync('OnDataChanged', entityType, changeType, entityId);
        });

        connection.onreconnecting((error) => {
            console.warn('[DataSync] Reconectando...', error);
        });

        connection.onreconnected((connectionId) => {
            console.log('[DataSync] Reconectado:', connectionId);
        });

        connection.onclose((error) => {
            console.error('[DataSync] Conexión cerrada:', error);
            window.dataSyncConnection = null;
        });

        await connection.start();
        console.log('[DataSync] Conectado al Hub');
        window.dataSyncConnection = connection;
    } catch (err) {
        console.error('[DataSync] Error conectando:', err);
        window.dataSyncConnection = null;
    }
};

window.disconnectDataSync = async function () {
    try {
        if (window.dataSyncConnection) {
            await window.dataSyncConnection.stop();
            console.log('[DataSync] Desconectado del Hub');
            window.dataSyncConnection = null;
        }
    } catch (err) {
        console.error('[DataSync] Error desconectando:', err);
    }
};
