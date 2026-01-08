/**
 * SGRRHH LOCAL - Keyboard Handler
 * Manejo de atajos de teclado estilo ForestechOil
 */
window.KeyboardHandler = (function () {
    let dotNetRef = null;
    let isEnabled = true;
    let currentFocusIndex = -1;
    let focusableElements = [];

    /**
     * Inicializa el manejador de teclado
     * @param {DotNetObjectReference} dotNetReference - Referencia al componente .NET
     */
    function initialize(dotNetReference) {
        dotNetRef = dotNetReference;
        document.addEventListener('keydown', handleKeyDown, true);
        console.log('[KeyboardHandler] Inicializado');
    }

    /**
     * Maneja eventos de tecla presionada
     * @param {KeyboardEvent} e - Evento de teclado
     */
    function handleKeyDown(e) {
        if (!isEnabled) return;

        // No capturar si está en un campo de texto y no es una tecla de función
        const activeElement = document.activeElement;
        const isInInput = activeElement && (
            activeElement.tagName === 'INPUT' ||
            activeElement.tagName === 'TEXTAREA' ||
            activeElement.tagName === 'SELECT'
        );

        // Permitir que las teclas de función funcionen incluso en inputs
        const isFunctionKey = e.key.startsWith('F') && /^F\d+$/.test(e.key);
        const isEscape = e.key === 'Escape';
        const isEnter = e.key === 'Enter';
        const isArrowKey = ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key);

        // Si está en input y no es tecla especial, dejar pasar
        if (isInInput && !isFunctionKey && !isEscape) {
            // Enter en input navega al siguiente campo
            if (isEnter && activeElement.tagName !== 'TEXTAREA') {
                e.preventDefault();
                focusNextElement();
                return;
            }
            return;
        }

        // Prevenir comportamiento por defecto para teclas de función
        if (isFunctionKey) {
            e.preventDefault();
        }

        // Navegación con flechas en tablas
        if (isArrowKey && !isInInput) {
            const table = document.querySelector('table tbody');
            if (table) {
                const rows = table.querySelectorAll('tr');
                const selectedRow = table.querySelector('tr.selected');
                let currentIndex = selectedRow ? Array.from(rows).indexOf(selectedRow) : -1;

                if (e.key === 'ArrowDown' && currentIndex < rows.length - 1) {
                    e.preventDefault();
                    if (rows[currentIndex + 1]) {
                        rows[currentIndex + 1].click();
                        rows[currentIndex + 1].scrollIntoView({ block: 'nearest' });
                    }
                } else if (e.key === 'ArrowUp' && currentIndex > 0) {
                    e.preventDefault();
                    if (rows[currentIndex - 1]) {
                        rows[currentIndex - 1].click();
                        rows[currentIndex - 1].scrollIntoView({ block: 'nearest' });
                    }
                }
                return;
            }
        }

        // Enviar evento a .NET
        if (dotNetRef) {
            const eventData = {
                key: e.key,
                code: e.code,
                ctrlKey: e.ctrlKey,
                altKey: e.altKey,
                shiftKey: e.shiftKey
            };

            dotNetRef.invokeMethodAsync('OnKeyPressed', eventData)
                .catch(err => console.error('[KeyboardHandler] Error:', err));
        }
    }

    /**
     * Enfoca el siguiente elemento focusable
     */
    function focusNextElement() {
        updateFocusableElements();
        
        const activeElement = document.activeElement;
        const currentIndex = focusableElements.indexOf(activeElement);
        
        if (currentIndex >= 0 && currentIndex < focusableElements.length - 1) {
            focusableElements[currentIndex + 1].focus();
        } else if (focusableElements.length > 0) {
            focusableElements[0].focus();
        }
    }

    /**
     * Actualiza la lista de elementos focusables
     */
    function updateFocusableElements() {
        const container = document.querySelector('.modal-box') || document.querySelector('.work-area');
        if (!container) {
            focusableElements = [];
            return;
        }

        focusableElements = Array.from(container.querySelectorAll(
            'input:not([disabled]):not([type="hidden"]), ' +
            'select:not([disabled]), ' +
            'textarea:not([disabled]), ' +
            'button:not([disabled])'
        )).filter(el => el.offsetParent !== null); // Solo elementos visibles
    }

    /**
     * Habilita o deshabilita el manejador
     * @param {boolean} enabled - Estado habilitado
     */
    function setEnabled(enabled) {
        isEnabled = enabled;
        console.log('[KeyboardHandler] Enabled:', enabled);
    }

    /**
     * Enfoca un elemento por su ID
     * @param {string} elementId - ID del elemento
     */
    function focusElement(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
            if (element.select) {
                element.select();
            }
        }
    }

    /**
     * Enfoca el primer elemento focusable en un contenedor
     * @param {string} containerId - ID del contenedor (opcional)
     */
    function focusFirst(containerId) {
        const container = containerId 
            ? document.getElementById(containerId) 
            : document.querySelector('.modal-box') || document.querySelector('.work-area');
        
        if (container) {
            const firstFocusable = container.querySelector(
                'input:not([disabled]):not([type="hidden"]), ' +
                'select:not([disabled]), ' +
                'textarea:not([disabled])'
            );
            if (firstFocusable) {
                firstFocusable.focus();
                if (firstFocusable.select) {
                    firstFocusable.select();
                }
            }
        }
    }

    /**
     * Selecciona una fila de la tabla
     * @param {number} index - Índice de la fila
     */
    function selectTableRow(index) {
        const table = document.querySelector('table tbody');
        if (table) {
            const rows = table.querySelectorAll('tr');
            if (rows[index]) {
                rows[index].click();
                rows[index].scrollIntoView({ block: 'nearest' });
            }
        }
    }

    /**
     * Limpia y libera recursos
     */
    function dispose() {
        document.removeEventListener('keydown', handleKeyDown, true);
        dotNetRef = null;
        isEnabled = false;
        console.log('[KeyboardHandler] Disposed');
    }

    // API pública
    return {
        initialize: initialize,
        setEnabled: setEnabled,
        focusElement: focusElement,
        focusFirst: focusFirst,
        focusNextElement: focusNextElement,
        selectTableRow: selectTableRow,
        dispose: dispose
    };
})();
