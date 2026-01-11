/**
 * SGRRHH LOCAL - Empleado Form Keyboard Handler
 * Manejo de navegación por teclado específico para el formulario de empleados
 */
window.EmpleadoFormKeyboard = (function () {
    let dotNetRef = null;
    let isActive = false;
    
    // Orden de campos para navegación
    const fieldOrder = [
        'field-codigo',
        'field-cedula', 
        'field-nombres',
        'field-apellidos',
        'field-fechanacimiento',
        'field-genero',
        'field-estadocivil',
        'field-direccion',
        'field-fechaingreso',
        'field-fecharetiro',
        'field-departamento',
        'field-cargo',
        'field-estado',
        'field-telefono',
        'field-email',
        'field-contactoemergencia',
        'field-telefonoemergencia',
        'field-observaciones'
    ];

    /**
     * Inicializa el manejador para el formulario de empleado
     * @param {DotNetObjectReference} dotNetReference - Referencia al componente .NET
     */
    function initialize(dotNetReference) {
        dotNetRef = dotNetReference;
        isActive = true;
        
        document.addEventListener('keydown', handleKeyDown, true);
        document.addEventListener('keypress', handleKeyPress, true);
        
        console.log('[EmpleadoFormKeyboard] Inicializado');
        
        // Enfocar primer campo después de un breve delay
        setTimeout(() => focusFirstField(), 100);
    }

    /**
     * Maneja el evento keydown
     * @param {KeyboardEvent} e 
     */
    function handleKeyDown(e) {
        if (!isActive) return;
        
        // Verificar si estamos en el modal del formulario
        const modal = document.querySelector('.hospital-modal-container');
        if (!modal) return;
        
        const activeElement = document.activeElement;
        const isInModal = modal.contains(activeElement);
        
        if (!isInModal && activeElement !== document.body) return;
        
        // F9 = Guardar
        if (e.key === 'F9') {
            e.preventDefault();
            e.stopPropagation();
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnF9Pressed');
            }
            return;
        }
        
        // ESC = Cancelar
        if (e.key === 'Escape') {
            e.preventDefault();
            e.stopPropagation();
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnEscPressed');
            }
            return;
        }
        
        // Enter = Siguiente campo (excepto en textarea)
        if (e.key === 'Enter') {
            const isTextarea = activeElement && activeElement.tagName === 'TEXTAREA';
            const isButton = activeElement && activeElement.tagName === 'BUTTON';
            
            if (isButton) {
                // Dejar que los botones funcionen normalmente
                return;
            }
            
            if (isTextarea) {
                // En textarea, Ctrl+Enter avanza
                if (e.ctrlKey) {
                    e.preventDefault();
                    focusNextField();
                }
                return;
            }
            
            e.preventDefault();
            e.stopPropagation();
            
            // Validar y avanzar
            const fieldId = activeElement ? activeElement.id : null;
            if (fieldId && fieldOrder.includes(fieldId)) {
                const fieldIndex = fieldOrder.indexOf(fieldId) + 1;
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnEnterPressed', fieldIndex)
                        .then(isValid => {
                            if (isValid) {
                                focusNextField();
                            }
                        });
                } else {
                    focusNextField();
                }
            } else {
                focusNextField();
            }
            return;
        }
    }

    /**
     * Maneja el evento keypress para filtrar caracteres
     * @param {KeyboardEvent} e 
     */
    function handleKeyPress(e) {
        if (!isActive) return;
        
        const activeElement = document.activeElement;
        if (!activeElement || activeElement.tagName !== 'INPUT') return;
        
        const inputId = activeElement.id;
        
        // Cédula: solo números
        if (inputId === 'field-cedula') {
            if (!/^\d$/.test(e.key)) {
                e.preventDefault();
                return;
            }
        }
        
        // Teléfonos: números, guiones, espacios, paréntesis, +
        if (inputId === 'field-telefono' || inputId === 'field-telefonoemergencia') {
            if (!/^[\d\s\-\+\(\)]$/.test(e.key)) {
                e.preventDefault();
                return;
            }
        }
        
        // Nombres: letras, espacios, acentos, guiones, apóstrofes
        if (inputId === 'field-nombres' || inputId === 'field-apellidos' || inputId === 'field-contactoemergencia') {
            if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-']$/.test(e.key)) {
                e.preventDefault();
                return;
            }
        }
    }

    /**
     * Enfoca el primer campo del formulario
     */
    function focusFirstField() {
        // Si está editando, el código es readonly, empezar desde cédula
        const codigoField = document.getElementById('field-codigo');
        if (codigoField && codigoField.readOnly) {
            const cedulaField = document.getElementById('field-cedula');
            if (cedulaField) {
                cedulaField.focus();
                cedulaField.select();
            }
        } else if (codigoField) {
            codigoField.focus();
            codigoField.select();
        }
    }

    /**
     * Enfoca el siguiente campo en la secuencia
     */
    function focusNextField() {
        const activeElement = document.activeElement;
        if (!activeElement) {
            focusFirstField();
            return;
        }
        
        const currentId = activeElement.id;
        const currentIndex = fieldOrder.indexOf(currentId);
        
        if (currentIndex === -1) {
            focusFirstField();
            return;
        }
        
        // Si es el último campo, invocar guardar
        if (currentIndex >= fieldOrder.length - 1) {
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnF9Pressed');
            }
            return;
        }
        
        // Buscar el siguiente campo habilitado
        for (let i = currentIndex + 1; i < fieldOrder.length; i++) {
            const nextField = document.getElementById(fieldOrder[i]);
            if (nextField && !nextField.disabled && !nextField.readOnly) {
                nextField.focus();
                if (nextField.select) {
                    nextField.select();
                }
                return;
            }
        }
    }

    /**
     * Enfoca un campo específico por su índice (1-based)
     * @param {number} fieldIndex 
     */
    function focusField(fieldIndex) {
        if (fieldIndex < 1 || fieldIndex > fieldOrder.length) return;
        
        const fieldId = fieldOrder[fieldIndex - 1];
        const field = document.getElementById(fieldId);
        
        if (field && !field.disabled) {
            field.focus();
            if (field.select) {
                field.select();
            }
        }
    }

    /**
     * Desactiva el manejador
     */
    function dispose() {
        isActive = false;
        document.removeEventListener('keydown', handleKeyDown, true);
        document.removeEventListener('keypress', handleKeyPress, true);
        dotNetRef = null;
        console.log('[EmpleadoFormKeyboard] Disposed');
    }

    // API pública
    return {
        initialize: initialize,
        focusFirstField: focusFirstField,
        focusNextField: focusNextField,
        focusField: focusField,
        dispose: dispose
    };
})();
