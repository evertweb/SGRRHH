// ========================================
// SGRRHH Local - Funciones JavaScript
// ========================================

/**
 * Descarga un archivo desde base64
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {string} mimeType - Tipo MIME del archivo (ej: 'application/pdf')
 * @param {string} base64Content - Contenido del archivo en base64
 */
window.downloadFile = function(fileName, mimeType, base64Content) {
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64Content}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

/**
 * Descarga un archivo Excel desde base64
 */
window.downloadFileFromBase64 = function(fileName, base64Content) {
    const mimeType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64Content}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

/**
 * Descarga un archivo desde bytes
 */
window.downloadFileFromBytes = function(fileName, contentType, byteArray) {
    const blob = new Blob([byteArray], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

/**
 * Enfoca un elemento por ID
 */
window.focusElement = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

/**
 * Enfoca un elemento, lo resalta con error y muestra mensaje inline
 */
window.focusFieldWithError = function(elementId, errorMessage) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    // Limpiar errores previos
    clearAllFieldErrors();
    
    // Agregar clase de error al campo
    element.classList.add('field-error');
    
    // Buscar el grupo padre del campo
    const formGroup = element.closest('.hospital-form-group');
    if (formGroup) {
        formGroup.classList.add('has-error');
        
        // Crear mensaje de error inline si no existe
        let errorEl = formGroup.querySelector('.field-error-message');
        if (!errorEl) {
            errorEl = document.createElement('div');
            errorEl.className = 'field-error-message';
            formGroup.appendChild(errorEl);
        }
        errorEl.textContent = errorMessage;
        errorEl.style.display = 'block';
    }
    
    // Hacer scroll al elemento y enfocar
    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setTimeout(() => element.focus(), 100);
    
    // Limpiar error cuando el usuario escriba
    element.addEventListener('input', function clearError() {
        element.classList.remove('field-error');
        if (formGroup) {
            formGroup.classList.remove('has-error');
            const errorEl = formGroup.querySelector('.field-error-message');
            if (errorEl) errorEl.style.display = 'none';
        }
        element.removeEventListener('input', clearError);
    }, { once: true });
};

/**
 * Limpia todos los errores de campos
 */
window.clearAllFieldErrors = function() {
    document.querySelectorAll('.field-error').forEach(el => el.classList.remove('field-error'));
    document.querySelectorAll('.has-error').forEach(el => el.classList.remove('has-error'));
    document.querySelectorAll('.field-error-message').forEach(el => el.style.display = 'none');
};

/**
 * Imprime el contenido de una página
 */
window.printPage = function() {
    window.print();
};

/**
 * Copia texto al portapapeles
 */
window.copyToClipboard = async function(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Error copiando al portapapeles:', err);
        return false;
    }
};

/**
 * Obtiene las dimensiones de la ventana
 */
window.getWindowDimensions = function() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

/**
 * Scroll suave a un elemento
 */
window.scrollToElement = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

// ========================================
// GESTIÓN DE CAMBIOS SIN GUARDAR
// ========================================

let unsavedChangesRef = null;

/**
 * Configura el aviso de cambios sin guardar al cerrar la ventana
 */
window.setupBeforeUnloadWarning = function(dotNetRef) {
    unsavedChangesRef = dotNetRef;
    window.addEventListener('beforeunload', handleBeforeUnload);
};

/**
 * Remueve el aviso de cambios sin guardar
 */
window.removeBeforeUnloadWarning = function() {
    window.removeEventListener('beforeunload', handleBeforeUnload);
    unsavedChangesRef = null;
};

function handleBeforeUnload(e) {
    if (unsavedChangesRef) {
        try {
            const hasChanges = unsavedChangesRef.invokeMethodAsync('HasUnsavedChanges');
            if (hasChanges) {
                e.preventDefault();
                e.returnValue = '¿Seguro que desea salir? Hay cambios sin guardar.';
                return e.returnValue;
            }
        } catch (err) {
            console.warn('Error checking unsaved changes:', err);
        }
    }
}

// ========================================
// GESTIÓN DE SESIÓN
// ========================================

let sessionActivityRef = null;
let sessionActivityTimeout = null;

/**
 * Configura el registro de actividad de sesión
 */
window.setupSessionActivity = function(dotNetRef) {
    sessionActivityRef = dotNetRef;
    
    // Registrar eventos de actividad
    document.addEventListener('click', registerSessionActivity);
    document.addEventListener('keypress', registerSessionActivity);
    document.addEventListener('mousemove', throttledActivityRegistration);
    document.addEventListener('scroll', throttledActivityRegistration);
};

/**
 * Remueve el registro de actividad de sesión
 */
window.removeSessionActivity = function() {
    document.removeEventListener('click', registerSessionActivity);
    document.removeEventListener('keypress', registerSessionActivity);
    document.removeEventListener('mousemove', throttledActivityRegistration);
    document.removeEventListener('scroll', throttledActivityRegistration);
    sessionActivityRef = null;
};

function registerSessionActivity() {
    if (sessionActivityRef) {
        try {
            sessionActivityRef.invokeMethodAsync('RegisterActivity');
        } catch (err) {
            console.warn('Error registering activity:', err);
        }
    }
}

function throttledActivityRegistration() {
    if (!sessionActivityTimeout) {
        sessionActivityTimeout = setTimeout(() => {
            registerSessionActivity();
            sessionActivityTimeout = null;
        }, 30000); // Throttle a cada 30 segundos para mousemove/scroll
    }
}

// ========================================
// NAVEGACIÓN CON TECLADO EN TABLAS
// ========================================

/**
 * Configura la navegación con teclado en una tabla
 */
window.setupTableKeyboardNavigation = function(tableId, dotNetRef) {
    const table = document.getElementById(tableId);
    if (!table) return;
    
    table.addEventListener('keydown', function(e) {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('SelectNextRow');
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('SelectPreviousRow');
        } else if (e.key === 'Enter') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OpenSelectedRow');
        } else if (e.key === 'Home') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('SelectFirstRow');
        } else if (e.key === 'End') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('SelectLastRow');
        }
    });
    
    // Hacer la tabla enfocable
    table.setAttribute('tabindex', '0');
};

/**
 * Enfoca una tabla para navegación con teclado
 */
window.focusTable = function(tableId) {
    const table = document.getElementById(tableId);
    if (table) {
        table.focus();
    }
};

/**
 * Scroll a la fila seleccionada de una tabla
 */
window.scrollToSelectedRow = function(tableId) {
    const table = document.getElementById(tableId);
    if (!table) return;
    
    const selectedRow = table.querySelector('tr.selected');
    if (selectedRow) {
        selectedRow.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
};

// ========================================
// NOTIFICACIONES DEL NAVEGADOR
// ========================================

/**
 * Solicita permiso para notificaciones del navegador
 */
window.requestNotificationPermission = async function() {
    if (!('Notification' in window)) {
        return 'not-supported';
    }
    
    if (Notification.permission === 'granted') {
        return 'granted';
    }
    
    const permission = await Notification.requestPermission();
    return permission;
};

/**
 * Muestra una notificación del navegador
 */
window.showBrowserNotification = function(title, body, icon) {
    if (!('Notification' in window) || Notification.permission !== 'granted') {
        return false;
    }
    
    const notification = new Notification(title, {
        body: body,
        icon: icon || '/images/logo.png',
        badge: '/images/badge.png'
    });
    
    notification.onclick = function() {
        window.focus();
        notification.close();
    };
    
    return true;
};

// ========================================
// LOCAL STORAGE HELPERS
// ========================================

/**
 * Guarda un valor en localStorage
 */
window.setLocalStorage = function(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
        return true;
    } catch (err) {
        console.error('Error saving to localStorage:', err);
        return false;
    }
};

/**
 * Obtiene un valor de localStorage
 */
window.getLocalStorage = function(key) {
    try {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : null;
    } catch (err) {
        console.error('Error reading from localStorage:', err);
        return null;
    }
};

/**
 * Elimina un valor de localStorage
 */
window.removeLocalStorage = function(key) {
    try {
        localStorage.removeItem(key);
        return true;
    } catch (err) {
        console.error('Error removing from localStorage:', err);
        return false;
    }
};

// ========================================
// KEYBOARD HANDLER
// ========================================

/**
 * Configura el manejador de teclado para una pagina
 */
window.addKeyboardHandler = function(dotNetRef) {
    document.addEventListener('keydown', function(e) {
        if (e.key === 'F5') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('HandleKeyPress', 'F5');
        } else if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('HandleKeyPress', 'Escape');
        }
    });
};

// ========================================
// AUTO-EXPAND TEXTAREAS
// ========================================

/**
 * Configura auto-expansión para todos los textareas con clase .auto-expand
 */
window.setupAutoExpandTextareas = function() {
    document.querySelectorAll('textarea.auto-expand').forEach(function(textarea) {
        autoExpandTextarea(textarea);
        textarea.addEventListener('input', function() {
            autoExpandTextarea(this);
        });
    });
};

/**
 * Auto-expande un textarea según su contenido
 */
function autoExpandTextarea(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = (textarea.scrollHeight) + 'px';
}

/**
 * Inicializa auto-expand en un textarea específico
 */
window.initAutoExpand = function(elementId) {
    const textarea = document.getElementById(elementId);
    if (textarea) {
        autoExpandTextarea(textarea);
        textarea.addEventListener('input', function() {
            autoExpandTextarea(this);
        });
    }
};

// ========================================
// FORM HELPERS
// ========================================

/**
 * Inicializa formularios del hospital (auto-expand, focus, etc.)
 */
window.initHospitalForm = function() {
    // Auto-expand para textareas
    document.querySelectorAll('.hospital-form-group textarea').forEach(function(textarea) {
        // Ajustar altura inicial
        if (textarea.value) {
            textarea.style.height = 'auto';
            textarea.style.height = Math.max(80, textarea.scrollHeight) + 'px';
        }
        
        // Ajustar al escribir
        textarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = Math.max(80, this.scrollHeight) + 'px';
        });
    });
};
