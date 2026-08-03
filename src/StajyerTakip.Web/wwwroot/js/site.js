// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Profil fotografi buyutme (WhatsApp tarzi lightbox) - .stk-avatar icindeki
// her <img> icin gecerli, hem kendi Profilim sayfasinda hem Stajyer/Mentor
// listelerindeki kucuk avatarlarda.
(function () {
    function kutuyuGetir() {
        var kutu = document.getElementById('stkLightbox');
        if (kutu) {
            return kutu;
        }

        kutu = document.createElement('div');
        kutu.id = 'stkLightbox';
        kutu.className = 'stk-lightbox';
        kutu.innerHTML = '<button type="button" class="stk-lightbox-kapat" aria-label="Kapat">&times;</button><img alt="" />';
        document.body.appendChild(kutu);

        kutu.addEventListener('click', function (e) {
            if (e.target === kutu || e.target.classList.contains('stk-lightbox-kapat')) {
                kutu.classList.remove('stk-acik');
            }
        });

        return kutu;
    }

    document.addEventListener('click', function (e) {
        var img = e.target.closest('.stk-avatar img');
        if (!img) {
            return;
        }

        var kutu = kutuyuGetir();
        var buyukResim = kutu.querySelector('img');
        buyukResim.src = img.src;
        buyukResim.alt = img.alt || '';
        kutu.classList.add('stk-acik');
    });

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') {
            return;
        }

        var kutu = document.getElementById('stkLightbox');
        if (kutu) {
            kutu.classList.remove('stk-acik');
        }
    });
})();
