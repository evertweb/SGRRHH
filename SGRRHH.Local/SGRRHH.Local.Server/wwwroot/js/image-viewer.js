/**
 * image-viewer.js
 * Interoperabilidad JavaScript para el visor de imágenes en ventana separada
 */

// Log inmediato para verificar que el script se carga
(function() {
    console.log('=== IMAGE-VIEWER.JS CARGADO ===');
    console.log('[ImageViewer] Script cargado correctamente - v3');
})();

window.imageViewerInterop = {
    // Referencia a la ventana del visor
    viewerWindow: null,
    
    // Datos pendientes para enviar
    pendingData: null,
    
    // Referencia al componente .NET del visor
    viewerComponent: null,
    
    /**
     * Inicializa el visor (llamado desde ImageViewer.razor)
     */
    initialize: function(dotNetRef) {
        console.log('[ImageViewer] initialize() llamado');
        this.viewerComponent = dotNetRef;
        
        // Cargar imágenes desde localStorage si existen
        var storedData = localStorage.getItem('imageViewer_data');
        if (storedData) {
            try {
                var data = JSON.parse(storedData);
                localStorage.removeItem('imageViewer_data');
                
                if (data.dataUrls && data.dataUrls.length > 0) {
                    dotNetRef.invokeMethodAsync('ReceiveAllPages', data.dataUrls, data.infos);
                }
            } catch (e) {
                console.error('Error loading image data:', e);
            }
        }
    },
    
    /**
     * Prepara los datos para el visor (llamado desde Blazor)
     * Retorna true si los datos se guardaron correctamente
     */
    prepareData: function(dataUrls, infos) {
        console.log('[ImageViewer] prepareData() llamado con', dataUrls.length, 'imágenes');
        
        try {
            // Guardar datos en localStorage
            localStorage.setItem('imageViewer_data', JSON.stringify({
                dataUrls: dataUrls,
                infos: infos
            }));
            console.log('[ImageViewer] Datos guardados en localStorage');
            return true;
        } catch (e) {
            console.error('[ImageViewer] Error guardando datos:', e);
            return false;
        }
    },
    
    /**
     * Abre la ventana del visor (debe llamarse desde un click handler directo)
     */
    openViewerWindow: function() {
        console.log('[ImageViewer] openViewerWindow() llamado');
        
        // Calcular tamaño óptimo (90% de la pantalla)
        var width = Math.min(window.screen.availWidth * 0.9, 1600);
        var height = Math.min(window.screen.availHeight * 0.9, 1000);
        var left = (window.screen.availWidth - width) / 2;
        var top = (window.screen.availHeight - height) / 2;
        
        var viewerUrl = window.location.origin + '/image-viewer';
        console.log('[ImageViewer] Abriendo ventana:', viewerUrl);
        
        this.viewerWindow = window.open(
            viewerUrl,
            'ImageViewer_' + Date.now(),
            'width=' + width + ',height=' + height + ',left=' + left + ',top=' + top + 
            ',menubar=no,toolbar=no,location=no,status=no,resizable=yes,scrollbars=no'
        );
        
        console.log('[ImageViewer] Resultado window.open:', this.viewerWindow);
        
        if (!this.viewerWindow) {
            console.error('[ImageViewer] BLOQUEADO - El navegador bloqueó la ventana');
            alert('El navegador bloqueó la ventana emergente. Por favor, permite las ventanas emergentes para este sitio.');
            localStorage.removeItem('imageViewer_data');
            return false;
        }
        
        console.log('[ImageViewer] Ventana abierta exitosamente');
        return true;
    },
    
    /**
     * Abre el visor con las imágenes proporcionadas
     * Esta función se mantiene por compatibilidad pero ahora usa un enfoque de 2 pasos
     */
    openWithImages: function(dataUrls, infos) {
        console.log('[ImageViewer] openWithImages() llamado con', dataUrls.length, 'imágenes');
        
        // Paso 1: Guardar datos
        if (!this.prepareData(dataUrls, infos)) {
            return false;
        }
        
        // Paso 2: Abrir ventana
        return this.openViewerWindow();
    },
    
    /**
     * Cierra el visor
     */
    closeViewer: function() {
        if (this.viewerWindow && !this.viewerWindow.closed) {
            this.viewerWindow.close();
        }
        this.viewerWindow = null;
    }
};
