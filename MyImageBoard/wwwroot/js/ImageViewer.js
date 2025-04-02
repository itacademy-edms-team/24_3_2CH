document.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById('imageModal');
    const modalImage = document.getElementById('modalImage');
    const modalDialog = document.querySelector('#imageModal .modal-dialog');

    modal.addEventListener('show.bs.modal', (event) => {
        const imageUrl = event.relatedTarget.getAttribute('data-bs-image');
        modalImage.src = imageUrl;

        modalImage.onload = () => {
            const imgWidth = modalImage.naturalWidth;
            const imgHeight = modalImage.naturalHeight;
            const maxWidth = window.innerWidth * 0.9;
            const maxHeight = window.innerHeight * 0.9;

            let newWidth = imgWidth;
            let newHeight = imgHeight;
            if (newWidth > maxWidth) {
                newHeight = (maxWidth / newWidth) * newHeight;
                newWidth = maxWidth;
            }
            if (newHeight > maxHeight) {
                newWidth = (maxHeight / newHeight) * newWidth;
                newHeight = maxHeight;
            }

            modalDialog.style.width = `${newWidth}px`;
            modalDialog.style.height = `${newHeight}px`;
        };
    });

    modal.addEventListener('hidden.bs.modal', () => {
        modalDialog.style.width = '';
        modalDialog.style.height = '';
    });
});