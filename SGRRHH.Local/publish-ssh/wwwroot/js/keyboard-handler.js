/**
 * SGRRHH LOCAL - Keyboard Handler
 * Manejo de atajos de teclado estilo ForestechOil
 */
window.KeyboardHandler = (function () {
    let dotNetRef = null;
    let isEnabled = true;
    let currentFocusIndex = -1;
    let focusableElements = [];
    let isInitialized = false;

    /**
     * Inicializa el manejador de teclado
     * @param {DotNetObjectReference} dotNetReference - Referencia al componente .NET
     */
    function initialize(dotNetReference) {
        // Evitar inicialización duplicada
        if (isInitialized) {
            console.log('[KeyboardHandler] Ya inicializado, actualizando referencia');
            dotNetRef = dotNetReference;
            return;
        }
        
        dotNetRef = dotNetReference;
        document.addEventListener('keydown', handleKeyDown, true);
        document.addEventListener('keypress', handleKeyPress, true);
        isInitialized = true;
        isEnabled = true;
        console.log('[KeyboardHandler] Inicializado correctamente');
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

        // Validar que e.key existe antes de usarlo
        if (!e.key) return;

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

            console.log('[KeyboardHandler] Enviando tecla:', e.key);
            dotNetRef.invokeMethodAsync('OnKeyPressed', eventData)
                .then(() => console.log('[KeyboardHandler] Tecla procesada:', e.key))
                .catch(err => console.error('[KeyboardHandler] Error al procesar tecla:', err));
        }
    }
    
    /**
     * Maneja eventos de keypress para filtrado de caracteres
     * @param {KeyboardEvent} e - Evento de teclado
     */
    function handleKeyPress(e) {
        if (!isEnabled) return;
        
        const activeElement = document.activeElement;
        if (!activeElement || activeElement.tagName !== 'INPUT') return;
        
        const inputId = activeElement.id;
        
        // Filtrado para campos de cédula (solo números)
        if (inputId === 'field-cedula') {
            if (!/^\d$/.test(e.key)) {
                e.preventDefault();
                return;
            }
        }
        
        // Filtrado para campos de teléfono (números, guiones, espacios, paréntesis, +)
        if (inputId === 'field-telefono' || inputId === 'field-telefonoemergencia') {
            if (!/^[\d\s\-\+\(\)]$/.test(e.key)) {
                e.preventDefault();
                return;
            }
        }
        
        // Filtrado para nombres y apellidos (letras, espacios, acentos)
        if (inputId === 'field-nombres' || inputId === 'field-apellidos' || inputId === 'field-contactoemergencia') {
            // Permitir letras (incluyendo acentuadas), espacios, guiones
            if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-']$/.test(e.key)) {
                e.preventDefault();
                return;
            }
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
        // Buscar en modal hospital o en contenedores estándar
        const container = document.querySelector('.hospital-modal-container') || 
                         document.querySelector('.modal-box') || 
                         document.querySelector('.work-area');
        if (!container) {
            focusableElements = [];
            return;
        }

        focusableElements = Array.from(container.querySelectorAll(
            'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
            'select:not([disabled]), ' +
            'textarea:not([disabled]), ' +
            'button:not([disabled])'
        )).filter(el => el.offsetParent !== null) // Solo elementos visibles
          .sort((a, b) => (parseInt(a.tabIndex) || 999) - (parseInt(b.tabIndex) || 999)); // Ordenar por tabindex
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
            : document.querySelector('.hospital-modal-container') ||
              document.querySelector('.modal-box') || 
              document.querySelector('.work-area');
        
        if (container) {
            const firstFocusable = container.querySelector(
                'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
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
     * Limpia y libera recursos (permite reinicialización)
     */
    function dispose() {
        document.removeEventListener('keydown', handleKeyDown, true);
        document.removeEventListener('keypress', handleKeyPress, true);
        dotNetRef = null;
        isEnabled = false;
        isInitialized = false; // Permite reinicializar
        console.log('[KeyboardHandler] Disposed');
    }
    
    /**
     * Actualiza solo la referencia de .NET sin re-registrar listeners
     */
    function updateReference(dotNetReference) {
        dotNetRef = dotNetReference;
        isEnabled = true;
        console.log('[KeyboardHandler] Referencia actualizada');
    }

    // API pública
    return {
        initialize: initialize,
        updateReference: updateReference,
        setEnabled: setEnabled,
        focusElement: focusElement,
        focusFirst: focusFirst,
        focusNextElement: focusNextElement,
        selectTableRow: selectTableRow,
        dispose: dispose
    };
})();
