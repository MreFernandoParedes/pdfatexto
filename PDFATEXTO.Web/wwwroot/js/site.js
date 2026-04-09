document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("ArchivoPdf");
    const fileMeta = document.querySelector(".file-meta");

    if (!input || !fileMeta) {
        return;
    }

    input.addEventListener("change", () => {
        const file = input.files && input.files.length > 0 ? input.files[0] : null;

        if (!file) {
            fileMeta.innerHTML = "";
            return;
        }

        fileMeta.innerHTML =
            '<span class="file-meta__label">Archivo seleccionado:</span>' +
            `<span class="file-meta__name">${file.name}</span>`;
    });
});
