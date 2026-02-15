function downloadFile(filename, base64Data) {
    downloadFileWithType(filename, base64Data, 'text/csv;charset=utf-8');
}

function downloadFileWithType(filename, base64Data, mimeType) {
    try {
        mimeType = mimeType || 'text/csv;charset=utf-8';
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const blob = new Blob([bytes], { type: mimeType });

        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Download error:', error);
    }
}