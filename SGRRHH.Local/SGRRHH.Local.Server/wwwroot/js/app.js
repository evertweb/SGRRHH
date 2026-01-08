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
