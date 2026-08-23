window.djTracksSessionsAudio = {
    init: (audio, dotnet) => {
        const report = (method, message) => dotnet.invokeMethodAsync(method, message).catch(() => { });
        audio.addEventListener('loadedmetadata', () => dotnet.invokeMethodAsync('AudioLoaded', audio.duration));
        audio.addEventListener('timeupdate', () => dotnet.invokeMethodAsync('AudioTime', audio.currentTime, audio.duration || 0));
        audio.addEventListener('pause', () => dotnet.invokeMethodAsync('AudioPaused'));
        audio.addEventListener('ended', () => dotnet.invokeMethodAsync('AudioEnded'));
        audio.addEventListener('error', () => report('AudioError', 'El navegador no pudo cargar este audio.'));
        audio.addEventListener('stalled', () => report('AudioUnavailable', 'El audio dejó de estar disponible.'));
    },
    command: async (audio, kind, url, value) => {
        try {
            if (kind === 'Load') {
                audio.src = url;
                audio.load();
                await audio.play();
            } else if (kind === 'Play') await audio.play();
            else if (kind === 'Pause') audio.pause();
            else if (kind === 'Stop') { audio.pause(); audio.removeAttribute('src'); audio.load(); }
            else if (kind === 'Seek') audio.currentTime = value;
            else if (kind === 'Volume') audio.volume = value;
        } catch (error) {
            throw new Error(error?.message || 'El navegador no pudo reproducir este audio.');
        }
    },
    setVolume: (audio, value) => audio.volume = value
};
