// Firebase JS SDK Interop para Blazor WASM
// Este archivo maneja toda la comunicación con Firebase desde JavaScript

// Variables globales para las instancias de Firebase
let app = null;
let auth = null;
let db = null;
let storage = null;
let currentUser = null;

/**
 * Inicializa Firebase con la configuración proporcionada
 * @param {object} config - Configuración de Firebase (apiKey, projectId, etc.)
 */
export async function initializeFirebase(config) {
    // Importar módulos de Firebase dinámicamente
    const { initializeApp } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js');
    const { getAuth, onAuthStateChanged } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js');
    const { getFirestore } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');
    const { getStorage } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-storage.js');

    app = initializeApp(config);
    auth = getAuth(app);
    storage = getStorage(app);

    // Si se especifica un databaseId, usarlo (para bases de datos nombradas)
    if (config.databaseId && config.databaseId !== '(default)') {
        db = getFirestore(app, config.databaseId);
        console.log('[Firebase] Usando base de datos nombrada:', config.databaseId);
    } else {
        db = getFirestore(app);
    }

    // Escuchar cambios en el estado de autenticación
    onAuthStateChanged(auth, (user) => {
        currentUser = user;
        if (user) {
            console.log('[Firebase] Usuario autenticado:', user.email, 'UID:', user.uid);
        } else {
            console.log('[Firebase] Usuario no autenticado');
        }
    });

    console.log('[Firebase] Inicializado correctamente para proyecto:', config.projectId);
    console.log('[Firebase] Storage bucket:', config.storageBucket || 'default');
}

/**
 * Espera a que el usuario esté autenticado
 * Útil para asegurar que las llamadas a Firestore tengan el token
 */
async function waitForAuth() {
    if (currentUser) return currentUser;
    if (!auth) return null;

    // Esperar un momento para que el estado de auth se propague
    return new Promise((resolve) => {
        const { onAuthStateChanged } = import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js')
            .then(module => {
                const unsubscribe = module.onAuthStateChanged(auth, (user) => {
                    unsubscribe();
                    resolve(user);
                });
            });

        // Timeout de seguridad
        setTimeout(() => resolve(currentUser), 1000);
    });
}

/**
 * Autenticación con email y password
 * @param {string} email 
 * @param {string} password 
 * @returns {object} Resultado de autenticación
 */
export async function signInWithEmail(email, password) {
    const { signInWithEmailAndPassword } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js');

    try {
        console.log('[Firebase] Intentando login con:', email);
        const userCredential = await signInWithEmailAndPassword(auth, email, password);
        const user = userCredential.user;
        currentUser = user; // Actualizar inmediatamente
        const idToken = await user.getIdToken();

        console.log('[Firebase] Login exitoso. UID:', user.uid);
        console.log('[Firebase] Token obtenido:', idToken ? 'Sí' : 'No');

        return {
            uid: user.uid,
            email: user.email,
            displayName: user.displayName || '',
            idToken: idToken,
            success: true,
            errorMessage: null
        };
    } catch (error) {
        console.error('[Firebase] Error de autenticación:', error);

        let errorMessage = 'Error de autenticación';
        switch (error.code) {
            case 'auth/user-not-found':
                errorMessage = 'Usuario no encontrado';
                break;
            case 'auth/wrong-password':
                errorMessage = 'Contraseña incorrecta';
                break;
            case 'auth/invalid-email':
                errorMessage = 'Email inválido';
                break;
            case 'auth/user-disabled':
                errorMessage = 'Usuario deshabilitado';
                break;
            case 'auth/too-many-requests':
                errorMessage = 'Demasiados intentos. Intente más tarde';
                break;
            case 'auth/invalid-credential':
                errorMessage = 'Credenciales inválidas. Verifique usuario y contraseña.';
                break;
            default:
                errorMessage = error.message;
        }

        return {
            uid: '',
            email: '',
            displayName: null,
            idToken: null,
            success: false,
            errorMessage: errorMessage
        };
    }
}

/**
 * Cierra la sesión actual
 */
export async function signOut() {
    const { signOut: firebaseSignOut } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js');
    await firebaseSignOut(auth);
    currentUser = null;
    console.log('[Firebase] Sesión cerrada');
}

/**
 * Obtiene el usuario actualmente autenticado
 * @returns {object|null} Usuario actual o null
 */
export async function getCurrentUser() {
    if (!auth) return null;

    // Verificar si hay usuario en memoria
    if (currentUser) {
        const idToken = await currentUser.getIdToken();
        return {
            uid: currentUser.uid,
            email: currentUser.email,
            displayName: currentUser.displayName || '',
            idToken: idToken,
            success: true,
            errorMessage: null
        };
    }

    // Verificar el estado actual de auth
    if (auth.currentUser) {
        currentUser = auth.currentUser;
        const idToken = await currentUser.getIdToken();
        return {
            uid: currentUser.uid,
            email: currentUser.email,
            displayName: currentUser.displayName || '',
            idToken: idToken,
            success: true,
            errorMessage: null
        };
    }

    return null;
}

/**
 * Verifica que hay un usuario autenticado antes de operaciones de Firestore
 */
function ensureAuthenticated() {
    const user = auth?.currentUser || currentUser;
    if (!user) {
        console.warn('[Firebase] ADVERTENCIA: Intentando acceder a Firestore sin usuario autenticado');
    } else {
        console.log('[Firebase] Usuario actual para Firestore:', user.email);
    }
    return user;
}

/**
 * Obtiene todos los documentos de una colección
 * @param {string} collectionPath - Ruta de la colección
 * @returns {Array} Array de documentos
 */
export async function getCollection(collectionPath) {
    const { collection, getDocs, getDocsFromServer } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    ensureAuthenticated();
    console.log('[Firebase] Obteniendo colección:', collectionPath);

    try {
        // Intentar primero desde el servidor para evitar problemas de "offline"
        let querySnapshot;
        try {
            querySnapshot = await getDocsFromServer(collection(db, collectionPath));
        } catch (serverError) {
            console.warn('[Firebase] Error desde servidor, intentando caché:', serverError.message);
            querySnapshot = await getDocs(collection(db, collectionPath));
        }

        const results = querySnapshot.docs.map(doc => ({
            _documentId: doc.id,
            ...doc.data()
        }));
        console.log('[Firebase] Colección obtenida:', collectionPath, '- Documentos:', results.length);
        return results;
    } catch (error) {
        console.error('[Firebase] Error obteniendo colección:', collectionPath, error);
        throw error;
    }
}

/**
 * Obtiene un documento específico por ID
 * @param {string} collectionPath - Ruta de la colección
 * @param {string} documentId - ID del documento
 * @returns {object|null} Documento o null
 */
export async function getDocument(collectionPath, documentId) {
    const { doc, getDoc, getDocFromServer } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    const user = ensureAuthenticated();
    console.log('[Firebase] Obteniendo documento:', collectionPath + '/' + documentId);
    console.log('[Firebase] Auth state al obtener doc:', user ? user.email : 'NO AUTENTICADO');

    try {
        const docRef = doc(db, collectionPath, documentId);

        // Intentar primero desde el servidor para evitar problemas de "offline"
        let docSnap;
        try {
            docSnap = await getDocFromServer(docRef);
        } catch (serverError) {
            console.warn('[Firebase] Error desde servidor, intentando caché:', serverError.message);
            docSnap = await getDoc(docRef);
        }

        if (docSnap.exists()) {
            console.log('[Firebase] Documento encontrado:', documentId);
            return { _documentId: docSnap.id, ...docSnap.data() };
        }
        console.log('[Firebase] Documento no existe:', documentId);
        return null;
    } catch (error) {
        console.error('[Firebase] Error obteniendo documento:', collectionPath + '/' + documentId, error);
        throw error;
    }
}

/**
 * Guarda/actualiza un documento con ID específico
 * @param {string} collectionPath - Ruta de la colección
 * @param {string} documentId - ID del documento
 * @param {object} data - Datos a guardar
 */
export async function setDocument(collectionPath, documentId, data) {
    const { doc, setDoc } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    ensureAuthenticated();

    const docRef = doc(db, collectionPath, documentId);
    await setDoc(docRef, data, { merge: true });
    console.log('[Firebase] Documento guardado:', collectionPath, documentId);
}

/**
 * Agrega un documento con ID auto-generado
 * @param {string} collectionPath - Ruta de la colección
 * @param {object} data - Datos a guardar
 * @returns {string} ID del documento creado
 */
export async function addDocument(collectionPath, data) {
    const { collection, addDoc } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    ensureAuthenticated();

    const docRef = await addDoc(collection(db, collectionPath), data);
    console.log('[Firebase] Documento agregado:', collectionPath, docRef.id);
    return docRef.id;
}

/**
 * Elimina un documento
 * @param {string} collectionPath - Ruta de la colección
 * @param {string} documentId - ID del documento
 */
export async function deleteDocument(collectionPath, documentId) {
    const { doc, deleteDoc } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    ensureAuthenticated();

    const docRef = doc(db, collectionPath, documentId);
    await deleteDoc(docRef);
    console.log('[Firebase] Documento eliminado:', collectionPath, documentId);
}

/**
 * Consulta documentos con un filtro simple
 * @param {string} collectionPath - Ruta de la colección
 * @param {string} field - Campo a filtrar
 * @param {string} op - Operador (==, !=, <, >, etc.)
 * @param {any} value - Valor a comparar
 * @returns {Array} Documentos que cumplen el filtro
 */
export async function queryCollection(collectionPath, field, op, value) {
    const { collection, query, where, getDocs, getDocsFromServer } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');

    ensureAuthenticated();
    console.log('[Firebase] Query:', collectionPath, field, op, value);

    try {
        const q = query(collection(db, collectionPath), where(field, op, value));

        // Intentar primero desde el servidor
        let querySnapshot;
        try {
            querySnapshot = await getDocsFromServer(q);
        } catch (serverError) {
            console.warn('[Firebase] Error desde servidor en query, intentando caché:', serverError.message);
            querySnapshot = await getDocs(q);
        }

        const results = querySnapshot.docs.map(doc => ({
            _documentId: doc.id,
            ...doc.data()
        }));
        console.log('[Firebase] Query completado. Resultados:', results.length);
        return results;
    } catch (error) {
        console.error('[Firebase] Error en query:', error);
        throw error;
    }
}

// ============================================
// FIREBASE STORAGE FUNCTIONS
// ============================================

/**
 * Sube un archivo a Firebase Storage desde Base64
 * @param {string} storagePath - Ruta donde guardar el archivo (ej: "documentos/empleado_1/cedula.pdf")
 * @param {string} base64Data - Datos del archivo en Base64
 * @param {string} contentType - Tipo MIME del archivo (ej: "application/pdf")
 * @returns {object} Resultado con downloadUrl y fullPath
 */
export async function uploadFileBase64(storagePath, base64Data, contentType) {
    const { ref, uploadBytes, getDownloadURL } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-storage.js');

    ensureAuthenticated();
    console.log('[Firebase Storage] Subiendo archivo a:', storagePath);

    try {
        // Convertir Base64 a Uint8Array
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        const storageRef = ref(storage, storagePath);
        const metadata = { contentType: contentType };

        await uploadBytes(storageRef, bytes, metadata);
        const downloadUrl = await getDownloadURL(storageRef);

        console.log('[Firebase Storage] Archivo subido exitosamente:', storagePath);
        return {
            success: true,
            downloadUrl: downloadUrl,
            fullPath: storagePath,
            errorMessage: null
        };
    } catch (error) {
        console.error('[Firebase Storage] Error subiendo archivo:', error);
        return {
            success: false,
            downloadUrl: null,
            fullPath: null,
            errorMessage: error.message
        };
    }
}

/**
 * Obtiene la URL de descarga de un archivo
 * @param {string} storagePath - Ruta del archivo en Storage
 * @returns {string|null} URL de descarga o null si no existe
 */
export async function getStorageUrl(storagePath) {
    const { ref, getDownloadURL } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-storage.js');

    ensureAuthenticated();
    console.log('[Firebase Storage] Obteniendo URL de:', storagePath);

    try {
        const storageRef = ref(storage, storagePath);
        const url = await getDownloadURL(storageRef);
        return url;
    } catch (error) {
        console.error('[Firebase Storage] Error obteniendo URL:', error);
        return null;
    }
}

/**
 * Elimina un archivo de Firebase Storage
 * @param {string} storagePath - Ruta del archivo a eliminar
 * @returns {boolean} true si se eliminó correctamente
 */
export async function deleteStorageFile(storagePath) {
    const { ref, deleteObject } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-storage.js');

    ensureAuthenticated();
    console.log('[Firebase Storage] Eliminando archivo:', storagePath);

    try {
        const storageRef = ref(storage, storagePath);
        await deleteObject(storageRef);
        console.log('[Firebase Storage] Archivo eliminado:', storagePath);
        return true;
    } catch (error) {
        console.error('[Firebase Storage] Error eliminando archivo:', error);
        return false;
    }
}

/**
 * Descarga un archivo de Firebase Storage como Base64
 * @param {string} storagePath - Ruta del archivo
 * @returns {object} Resultado con base64Data y contentType
 */
export async function downloadFileBase64(storagePath) {
    const { ref, getDownloadURL, getMetadata } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-storage.js');

    ensureAuthenticated();
    console.log('[Firebase Storage] Descargando archivo:', storagePath);

    try {
        const storageRef = ref(storage, storagePath);
        const url = await getDownloadURL(storageRef);
        const metadata = await getMetadata(storageRef);

        // Usar fetch para obtener los bytes
        const response = await fetch(url);
        const blob = await response.blob();

        // Convertir a Base64
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64 = reader.result.split(',')[1]; // Remover el prefijo "data:..."
                resolve({
                    success: true,
                    base64Data: base64,
                    contentType: metadata.contentType,
                    size: metadata.size,
                    errorMessage: null
                });
            };
            reader.readAsDataURL(blob);
        });
    } catch (error) {
        console.error('[Firebase Storage] Error descargando archivo:', error);
        return {
            success: false,
            base64Data: null,
            contentType: null,
            size: 0,
            errorMessage: error.message
        };
    }
}

// ============================================
// UTILITY FUNCTIONS FOR BLAZOR
// ============================================

/**
 * Descarga un archivo al dispositivo del usuario
 * @param {string} filename - Nombre del archivo
 * @param {string} contentType - Tipo MIME
 * @param {Uint8Array} bytes - Bytes del archivo
 */
export function downloadFile(filename, contentType, bytes) {
    const blob = new Blob([bytes], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

/**
 * Descarga un archivo de texto al dispositivo del usuario
 * @param {string} filename - Nombre del archivo
 * @param {string} content - Contenido del archivo como texto
 * @param {string} contentType - Tipo MIME (ej: "text/csv;charset=utf-8")
 */
export function downloadTextFile(filename, content, contentType) {
    // Agregar BOM para UTF-8 en archivos CSV (para que Excel lo reconozca correctamente)
    const bom = contentType.includes('csv') ? '\uFEFF' : '';
    const blob = new Blob([bom + content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    console.log('[Util] Archivo descargado:', filename);
}

// Hacer disponible globalmente para llamadas directas desde Blazor
window.downloadFile = function(filename, content, contentType) {
    // Si es un string, usar la versión de texto
    if (typeof content === 'string') {
        downloadTextFile(filename, content, contentType);
    } else {
        // Si son bytes, usar la versión original
        downloadFile(filename, contentType, content);
    }
};
window.downloadTextFile = downloadTextFile;
