/**
 * SGRRHH LOCAL - Hospital Modal Handler
 * Manejo de modales estilo hospitalario
 */
window.hospitalModal = (function () {
    
    let dotNetHelper = null;
    let keyHandler = null;

    /**
     * Inicializa el manejador de modal con referencia a .NET para callbacks
     */
    function init(dotNetRef) {
        dotNetHelper = dotNetRef;
        
        // Enfocar primer elemento focusable en el modal
        setTimeout(() => {
            const modal = document.querySelector('.hospital-modal-box');
            if (modal) {
                const firstInput = modal.querySelector(
                    'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
                    'select:not([disabled]), ' +
                    'textarea:not([disabled])'
                );
                if (firstInput) {
                    firstInput.focus();
                }
                
                // Inicializar auto-expand en textareas
                modal.querySelectorAll('textarea').forEach(function(textarea) {
                    if (textarea.value) {
                        textarea.style.height = 'auto';
                        textarea.style.height = Math.max(80, textarea.scrollHeight) + 'px';
                    }
                    
                    textarea.addEventListener('input', function() {
                        this.style.height = 'auto';
                        this.style.height = Math.max(80, this.scrollHeight) + 'px';
                    });
                });
            }
        }, 50);
        
        // Registrar manejador de teclas
        if (keyHandler) {
            document.removeEventListener('keydown', keyHandler);
        }
        
        keyHandler = function(e) {
            const modal = document.querySelector('.hospital-modal-overlay');
            if (!modal) return;
            
            // F5 = Guardar (prevenir recarga de página)
            if (e.key === 'F5') {
                e.preventDefault();
                e.stopPropagation();
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnKeyboardSave');
                }
                return false;
            }
            
            // F8 = Cancelar
            if (e.key === 'F8') {
                e.preventDefault();
                e.stopPropagation();
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnKeyboardCancel');
                }
                return false;
            }
            
            // ESC = Salir/Cerrar
            if (e.key === 'Escape') {
                e.preventDefault();
                e.stopPropagation();
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnKeyboardCancel');
                }
                return false;
            }
        };
        
        document.addEventListener('keydown', keyHandler);
    }

    /**
     * Limpia los event listeners al cerrar el modal
     */
    function dispose() {
        if (keyHandler) {
            document.removeEventListener('keydown', keyHandler);
            keyHandler = null;
        }
        dotNetHelper = null;
    }

    /**
     * Cierra el modal
     */
    function close() {
        dispose();
        const overlay = document.querySelector('.hospital-modal-overlay');
        if (overlay) {
            overlay.click();
        }
    }

    // API pública
    return {
        init: init,
        close: close,
        dispose: dispose
    };
})();
