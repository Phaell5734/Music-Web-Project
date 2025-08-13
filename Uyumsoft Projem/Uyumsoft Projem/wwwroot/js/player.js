$(function () {
    var $global = $('#globalPlayer'),
        globalAudio = $('#audioPlayer')[0],
        $cover = $('#playerCover'),
        $title = $('#playerTitle'),
        $artist = $('#playerArtist'),
        $close = $('#playerClose'),
        $progressBar = $('#globalPlayer .progress').length
            ? $('#globalPlayer .progress')
            : $('<div class="progress w-50 mx-3"><div class="progress-bar"></div></div>')
                .insertAfter($('#audioPlayer'));

    // Sadece player.js'in kendi functionality'si için
    // .btn-play event'i jplayerInit.js'de hallediliyor artık

    // 2) progress bar
    if (globalAudio) {
        globalAudio.ontimeupdate = function () {
            var pct = (globalAudio.currentTime / globalAudio.duration) * 100 || 0;
            $progressBar.find('.progress-bar').css('width', pct + '%');
        };
    }

    // 4) volume persistence
    var savedVol = localStorage.getItem('globalVolume');
    if (savedVol !== null && globalAudio) {
        globalAudio.volume = +savedVol;
    }
    if (globalAudio) {
        globalAudio.onvolumechange = function () {
            localStorage.setItem('globalVolume', globalAudio.volume);
        };
    }
});
