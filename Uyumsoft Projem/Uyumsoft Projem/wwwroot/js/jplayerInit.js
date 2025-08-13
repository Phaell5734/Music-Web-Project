// jPlayer Initialization -> HTML5 Audio + Playlist Manager
// wwwroot/js/jplayerInit.js
(function($) {
    console.log("HTML5 Audio + Playlist Manager yüklendi");
    
    // === GLOBAL DEÐÝÞKENLER ===
    var $globalBar = $('#globalPlayer'),
        audio = $('#audioPlayer')[0],
        $cover = $('#playerCover'),
        $title = $('#playerTitle'),
        $artist = $('#playerArtist');

    // Global playlist durumu
    window.GlobalPlaylist = {
        songs: [],
        currentIndex: 0,
        isShuffleMode: false,
        shuffledIndices: [],
        playlistId: null,
        playlistTitle: null,
        
        // Playlist durumunu sessionStorage'a kaydet
        saveState: function() {
            console.log("Saving playlist state:", this.playlistTitle, "songs:", this.songs.length);
            sessionStorage.setItem('globalPlaylistState', JSON.stringify({
                songs: this.songs,
                currentIndex: this.currentIndex,
                isShuffleMode: this.isShuffleMode,
                shuffledIndices: this.shuffledIndices,
                playlistId: this.playlistId,
                playlistTitle: this.playlistTitle
            }));
        },
        
        // Playlist durumunu sessionStorage'dan yükle
        loadState: function() {
            var state = sessionStorage.getItem('globalPlaylistState');
            if (state) {
                try {
                    state = JSON.parse(state);
                    this.songs = state.songs || [];
                    this.currentIndex = state.currentIndex || 0;
                    this.isShuffleMode = state.isShuffleMode || false;
                    this.shuffledIndices = state.shuffledIndices || [];
                    this.playlistId = state.playlistId || null;
                    this.playlistTitle = state.playlistTitle || null;
                    console.log("Loaded playlist state:", this.playlistTitle, "songs:", this.songs.length);
                    return true;
                } catch (e) {
                    console.error('Failed to load playlist state:', e);
                }
            }
            return false;
        },
        
        // Sonraki þarkýya geç
        playNext: function() {
            console.log("playNext called, playlist:", this.playlistTitle, "songs:", this.songs.length);
            if (this.songs.length === 0) return false;
            
            if (this.currentIndex < this.songs.length - 1) {
                this.currentIndex++;
            } else {
                this.currentIndex = 0; // Baþa dön
            }
            
            console.log("Moving to song index:", this.currentIndex);
            this.playCurrentSong();
            return true;
        },
        
        // Önceki þarkýya geç
        playPrevious: function() {
            console.log("playPrevious called, playlist:", this.playlistTitle, "songs:", this.songs.length);
            if (this.songs.length === 0) return false;
            
            if (this.currentIndex > 0) {
                this.currentIndex--;
            } else {
                this.currentIndex = this.songs.length - 1; // Sona git
            }
            
            console.log("Moving to song index:", this.currentIndex);
            this.playCurrentSong();
            return true;
        },
        
        // Mevcut þarkýyý çal
        playCurrentSong: function() {
            console.log("playCurrentSong called, currentIndex:", this.currentIndex);
            if (this.songs.length === 0) {
                console.log("No songs in playlist");
                return false;
            }
            
            var actualIndex = this.isShuffleMode ? 
                this.shuffledIndices[this.currentIndex] : this.currentIndex;
            var song = this.songs[actualIndex];
            
            console.log("Playing song:", song);
            
            if (song && audio) {
                audio.src = song.filePath;
                audio.currentTime = 0;
                
                // Set current song ID for tracking
                currentSongId = song.songId;
                
                audio.play();
                
                $cover.attr('src', song.imagePath);
                $title.text(song.title);
                $artist.text(this.playlistTitle ? 'Playlist: ' + this.playlistTitle : (song.artist || ''));
                
                $globalBar.slideDown(200);
                this.saveState();
                
                // Trigger custom event for playlist pages
                $(document).trigger('playlistSongChanged', {
                    playlistId: this.playlistId,
                    songIndex: actualIndex,
                    song: song
                });
                
                console.log("Song started playing successfully");
                return true;
            }
            console.log("Failed to play song - audio or song missing");
            return false;
        },
        
        // Playlist'i shuffle et
        shuffle: function() {
            this.shuffledIndices = [];
            for (var i = 0; i < this.songs.length; i++) {
                this.shuffledIndices.push(i);
            }
            
            // Fisher-Yates shuffle
            for (var i = this.shuffledIndices.length - 1; i > 0; i--) {
                var j = Math.floor(Math.random() * (i + 1));
                var temp = this.shuffledIndices[i];
                this.shuffledIndices[i] = this.shuffledIndices[j];
                this.shuffledIndices[j] = temp;
            }
            this.saveState();
        },
        
        // Playlist'i temizle
        clear: function() {
            console.log("Clearing playlist");
            this.songs = [];
            this.currentIndex = 0;
            this.isShuffleMode = false;
            this.shuffledIndices = [];
            this.playlistId = null;
            this.playlistTitle = null;
            sessionStorage.removeItem('globalPlaylistState');
        },
        
        // Yeni playlist yükle
        loadPlaylist: function(songs, playlistId, playlistTitle, startIndex, shuffleMode) {
            console.log("Loading playlist:", {
                songsCount: songs.length,
                playlistId: playlistId,
                playlistTitle: playlistTitle,
                startIndex: startIndex,
                shuffleMode: shuffleMode
            });
            
            this.songs = songs;
            this.currentIndex = startIndex || 0;
            this.isShuffleMode = shuffleMode || false;
            this.playlistId = playlistId;
            this.playlistTitle = playlistTitle;
            
            if (this.isShuffleMode) {
                this.shuffle();
            }
            
            this.saveState();
            console.log("Playlist loaded successfully");
        }
    };

    // === 1) sessionStorage'dan önceki çalma durumunu geri yükle ===
    (function restorePlayer() {
        // Playlist durumunu yükle
        window.GlobalPlaylist.loadState();
        
        // Player durumunu yükle
        var state = sessionStorage.getItem('globalPlayerState');
        if (!state) return;
        try {
            state = JSON.parse(state);
            if (state.src) {
                $cover.attr('src', state.cover || '');
                $title.text(state.title || '');
                $artist.text(state.artist || '');

                audio.src = state.src;
                audio.currentTime = state.time || 0;

                if (state.isPlaying) {
                    audio.play();
                    $globalBar.show();
                }
            }
        } catch (e) {
            console.error('Restore player state failed:', e);
        }
    })();

    // === 2) Audio events ===
    if (audio) {
        audio.addEventListener('play', function() {
            $('#playerPlayPause i').removeClass('fa-play').addClass('fa-pause');
            if (currentSongId) {
                startPlayTracking(currentSongId);
            }
            savePlayerState();
        });

        audio.addEventListener('pause', function() {
            $('#playerPlayPause i').removeClass('fa-pause').addClass('fa-play');
            stopPlayTracking();
            savePlayerState();
        });

        audio.addEventListener('timeupdate', function() {
            savePlayerState();
        });

        // Audio bitince sonraki þarkýya geç
        audio.addEventListener('ended', function() {
            console.log('Audio ended, trying to play next song...');
            stopPlayTracking();
            if (!window.GlobalPlaylist.playNext()) {
                console.log('No next song in playlist');
            }
        });
    }

    function savePlayerState() {
        var isPlaying = !audio.paused && !audio.ended;
        sessionStorage.setItem('globalPlayerState', JSON.stringify({
            src: audio.src,
            time: audio.currentTime,
            isPlaying: isPlaying,
            cover: $cover.attr('src'),
            title: $title.text(),
            artist: $artist.text()
        }));
    }

    // === 3) Tek þarký çalma butonlarý ===
    $(document).on('click', '.btn-play', function() {
        console.log("Single song play button clicked");
        var url = $(this).data('url');
        var cover = $(this).data('cover');
        var title = $(this).data('title');
        var artist = $(this).data('artist');
        var songId = $(this).data('song-id') || $(this).closest('[data-song-id]').data('song-id');
        
        // Playlist'i temizle (tek þarký çalma)
        window.GlobalPlaylist.clear();
        
        if (audio) {
            audio.src = url;
            audio.currentTime = 0;
            currentSongId = songId;
            audio.play();
            
            $cover.attr('src', cover);
            $title.text(title);
            $artist.text(artist || '');
            
            $globalBar.slideDown(200);
        }
    });

    // === 4) HTML5 Audio kontrolleri ===
    $('#playerPrevious').on('click', function() {
        console.log("Previous button clicked");
        window.GlobalPlaylist.playPrevious();
    });

    $('#playerNext').on('click', function() {
        console.log("Next button clicked");
        window.GlobalPlaylist.playNext();
    });

    $('#playerPlayPause').on('click', function() {
        if (audio && audio.src) {
            if (audio.paused) {
                audio.play();
            } else {
                audio.pause();
            }
        }
    });

    // Close button
    $('#playerClose').on('click', function() {
        if (audio) {
            audio.pause();
        }
        $globalBar.slideUp(200);
        sessionStorage.removeItem('globalPlayerState');
        window.GlobalPlaylist.clear();
    });

    console.log("HTML5 Audio + Playlist Manager initialization complete");

})(jQuery);

