"""Composes the game's music: small chiptune-style synth loops, written as OGG Vorbis files into
Assets/Resources/Music. Every note comes from the patterns below, so the music is original and
needs no license. Regenerate with:  python Tools/compose_music.py   (needs numpy and soundfile)

Loops are seamless: each track renders whole bars, and any note tails that run past the end
wrap around to the start.
"""
import os
import numpy as np
import soundfile as sf

RATE = 22050
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Music")
rng = np.random.default_rng(14)

NOTE_INDEX = {"C": 0, "C#": 1, "D": 2, "D#": 3, "E": 4, "F": 5, "F#": 6, "G": 7, "G#": 8, "A": 9, "A#": 10, "B": 11}
SCALES = {
    "minor": [0, 2, 3, 5, 7, 8, 10],
    "harmonic": [0, 2, 3, 5, 7, 8, 11],
    "phrygian": [0, 1, 3, 5, 7, 8, 10],
    "dorian": [0, 2, 3, 5, 7, 9, 10],
    "lydian": [0, 2, 4, 6, 7, 9, 11],
}


def freq(midi):
    return 440.0 * 2 ** ((midi - 69) / 12.0)


class Key:
    def __init__(self, root, scale, octave=4):
        self.root = NOTE_INDEX[root] + 12 * (octave + 1)
        self.steps = SCALES[scale]

    def degree(self, d, octave=0):
        """Scale degree d (0-based, may be negative or above 6) to a MIDI note."""
        o, i = divmod(d, len(self.steps))
        return self.root + self.steps[i] + 12 * (o + octave)


def env(n, attack=0.005, release=0.1, sustain=1.0):
    t = np.arange(n) / RATE
    a = np.clip(t / max(attack, 1e-4), 0, 1)
    r = np.clip((n / RATE - t) / max(release, 1e-4), 0, 1)
    return a * r * sustain


def osc(kind, f, n, duty=0.5, vibrato=0.0):
    t = np.arange(n) / RATE
    if vibrato:
        f = f * (1 + vibrato * np.sin(2 * np.pi * 5.5 * t))
    phase = np.cumsum(np.full(n, f) if np.isscalar(f) else f) / RATE
    phase -= np.floor(phase)
    if kind == "square":
        return np.where(phase < duty, 1.0, -1.0)
    if kind == "triangle":
        return 4 * np.abs(phase - 0.5) - 1
    if kind == "saw":
        return 2 * phase - 1
    return np.sin(2 * np.pi * phase)


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / RATE)
    y = np.empty_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc
        y[i] = acc
    return y


class Track:
    def __init__(self, bpm, bars, beats=4, loop=True):
        self.bpm = bpm
        self.beat = 60.0 / bpm
        self.length = int(bars * beats * self.beat * RATE)
        self.buf = np.zeros(self.length + RATE * 4)
        self.loop = loop

    def add(self, start_beat, signal, gain=1.0):
        s = int(start_beat * self.beat * RATE)
        self.buf[s:s + len(signal)] += signal * gain

    def note(self, start_beat, beats, midi, kind="square", gain=0.2, duty=0.5, attack=0.005, release=0.08, vibrato=0.0, cutoff=None):
        n = int(beats * self.beat * RATE + release * RATE)
        sig = osc(kind, freq(midi), n, duty, vibrato) * env(n, attack, release)
        if cutoff:
            sig = lowpass(sig, cutoff)
        self.add(start_beat, sig, gain)

    def pad(self, start_beat, beats, midis, gain=0.08, cutoff=1400):
        n = int(beats * self.beat * RATE)
        sig = np.zeros(n)
        for m in midis:
            for det in (-0.12, 0.0, 0.12):
                sig += osc("saw", freq(m + det), n)
        sig = lowpass(sig / (3 * len(midis)), cutoff) * env(n, attack=0.25, release=0.35)
        self.add(start_beat, sig, gain)

    def kick(self, b, gain=0.6):
        n = int(0.18 * RATE)
        t = np.arange(n) / RATE
        f = 120 * np.exp(-t * 28) + 42
        self.add(b, np.sin(2 * np.pi * np.cumsum(f) / RATE) * np.exp(-t * 18), gain)

    def snare(self, b, gain=0.25):
        n = int(0.16 * RATE)
        t = np.arange(n) / RATE
        noise = rng.uniform(-1, 1, n)
        body = np.sin(2 * np.pi * 190 * t)
        self.add(b, (noise * 0.8 + body * 0.4) * np.exp(-t * 22), gain)

    def hat(self, b, gain=0.07, open_=False):
        n = int((0.12 if open_ else 0.04) * RATE)
        noise = rng.uniform(-1, 1, n)
        noise = noise - lowpass(noise, 5000)
        self.add(b, noise * np.exp(-np.arange(n) / RATE * (18 if open_ else 70)), gain)

    def clang(self, b, gain=0.18):
        """A metal hit: inharmonic partials with a fast decay."""
        n = int(0.6 * RATE)
        t = np.arange(n) / RATE
        sig = sum(np.sin(2 * np.pi * f * t) * a for f, a in ((523, 1), (1187, 0.6), (1771, 0.4), (2410, 0.3)))
        self.add(b, sig * np.exp(-t * 7) / 2.3, gain)

    def render(self, name):
        out = self.buf[:self.length].copy()
        tail = self.buf[self.length:]
        if self.loop:
            out[:len(tail)] += tail[:len(out)]
        else:
            out = self.buf[:self.length + RATE].copy()
        out = np.tanh(out * 1.2)
        peak = np.max(np.abs(out)) or 1.0
        out = out / peak * 0.89
        os.makedirs(OUT, exist_ok=True)
        path = os.path.join(OUT, name + ".ogg")
        # Written in blocks: one large Vorbis write can crash libsndfile on Windows.
        data = out.astype(np.float32)
        with sf.SoundFile(path, "w", RATE, 1, format="OGG", subtype="VORBIS") as f:
            for i in range(0, len(data), 8192):
                f.write(data[i:i + 8192])
        print("wrote", os.path.normpath(path), "%.1fs" % (len(out) / RATE))


def chords(key, degrees, octave=0):
    return [[key.degree(d, octave), key.degree(d + 2, octave), key.degree(d + 4, octave)] for d in degrees]


def title():
    k = Key("A", "minor", 3)
    t = Track(100, 16)
    prog = [0, 5, 3, 4] * 4
    for bar, d in enumerate(prog):
        t.pad(bar * 4, 4, chords(k, [d], 1)[0], gain=0.09)
        t.note(bar * 4, 3.8, k.degree(d, -1), "triangle", 0.28, release=0.2)
        for s in range(8):
            t.note(bar * 4 + s * 0.5, 0.45, k.degree(d + [0, 2, 4, 7, 4, 2, 0, 2][s], 1), "square", 0.06, duty=0.25)
        if bar >= 4:
            t.kick(bar * 4); t.kick(bar * 4 + 2.5)
            t.snare(bar * 4 + 1); t.snare(bar * 4 + 3)
    melody = [4, 4, 3, 2, 3, 2, 0, -1, 0, 2, 4, 5, 4, 2, 4, None]
    for i, m in enumerate(melody * 2):
        if m is not None and i >= 16:
            t.note(i * 2 + 0.0, 1.8, k.degree(m, 2), "square", 0.07, duty=0.5, vibrato=0.006, release=0.15)
    t.render("title")


def hub():
    k = Key("D", "dorian", 3)
    t = Track(88, 12)
    prog = [0, 3, 4, 3] * 3
    for bar, d in enumerate(prog):
        t.pad(bar * 4, 4, chords(k, [d], 1)[0], gain=0.08, cutoff=1000)
        t.note(bar * 4, 1.9, k.degree(d, -1), "triangle", 0.25)
        t.note(bar * 4 + 2, 1.9, k.degree(d + 4, -1), "triangle", 0.2)
        for s in [0, 1.5, 3]:
            t.note(bar * 4 + s, 0.4, k.degree(d + 4, 2), "square", 0.045, duty=0.125, release=0.3)
        for h in range(4):
            t.hat(bar * 4 + h + 0.5, 0.04)
    t.render("hub")


def catacombs():
    k = Key("E", "phrygian", 2)
    t = Track(76, 12)
    for bar in range(12):
        d = [0, 0, 1, 0, 5, 5, 1, 0, 0, 3, 1, 0][bar]
        t.pad(bar * 4, 4, [k.degree(d, 1), k.degree(d + 2, 1), k.degree(d + 4, 1)], gain=0.07, cutoff=700)
        t.note(bar * 4, 4, k.degree(0, 0), "sine", 0.22, attack=0.4, release=0.5)
        if bar % 2 == 1:
            t.kick(bar * 4, 0.35)
        bell = [4, 3, 1, 0, 1, 3, 5, 4][bar % 8]
        t.note(bar * 4 + 1, 2.5, k.degree(bell, 3), "sine", 0.08, release=1.2, vibrato=0.004)
        t.note(bar * 4 + 1, 2.5, k.degree(bell, 3) + 12, "sine", 0.02, release=1.0)
    t.render("catacombs")


def torture():
    k = Key("C", "harmonic", 2)
    t = Track(108, 16)
    prog = [0, 0, 5, 4] * 4
    for bar, d in enumerate(prog):
        for s in range(8):
            t.note(bar * 4 + s * 0.5, 0.4, k.degree(d + (0 if s % 2 == 0 else 4), 0), "square", 0.12, duty=0.3, cutoff=1500)
        t.pad(bar * 4, 4, chords(k, [d], 1)[0], gain=0.06, cutoff=900)
        t.kick(bar * 4); t.kick(bar * 4 + 2)
        t.snare(bar * 4 + 1, 0.2); t.snare(bar * 4 + 3, 0.2)
        if bar % 2 == 0:
            t.clang(bar * 4 + 3.5, 0.12)
        if bar >= 8:
            t.note(bar * 4, 3.5, k.degree([4, 3, 2, 6][bar % 4], 2), "saw", 0.05, vibrato=0.005, release=0.3, cutoff=2200)
    t.render("torture")


def cellars():
    k = Key("G", "minor", 2)
    t = Track(118, 16)
    prog = [0, 3, 4, 0] * 4
    groove = [0, None, 4, 0, None, 4, 2, None]
    for bar, d in enumerate(prog):
        for s, g in enumerate(groove):
            if g is not None:
                t.note(bar * 4 + s * 0.5, 0.35, k.degree(d + g, 0), "square", 0.13, duty=0.5, cutoff=1300)
        t.kick(bar * 4); t.kick(bar * 4 + 1.5); t.kick(bar * 4 + 2.5)
        t.snare(bar * 4 + 1); t.snare(bar * 4 + 3)
        for h in range(8):
            t.hat(bar * 4 + h * 0.5 + 0.25, 0.04)
        lead = [[4, 5, 4, 2], [3, 2, 0, 2], [4, 6, 7, 6], [4, 2, 1, 0]][bar % 4]
        for i, m in enumerate(lead):
            t.note(bar * 4 + i, 0.6, k.degree(m, 2), "square", 0.05, duty=0.25, release=0.1)
    t.render("cellars")


def forge():
    k = Key("D", "minor", 1)
    t = Track(128, 16)
    prog = [0, 0, 6, 5] * 4
    for bar, d in enumerate(prog):
        for s in range(16):
            if s % 4 != 3:
                t.note(bar * 4 + s * 0.25, 0.22, k.degree(d, 1), "saw", 0.09, cutoff=900)
        for b in range(4):
            t.kick(bar * 4 + b, 0.55)
        t.snare(bar * 4 + 1, 0.22); t.snare(bar * 4 + 3, 0.22)
        t.clang(bar * 4 + 0.75, 0.1); t.clang(bar * 4 + 2.75, 0.08)
        t.pad(bar * 4, 4, chords(k, [d], 2)[0], gain=0.05, cutoff=1200)
        if bar >= 4:
            riff = [0, 3, 2, 3, 5, 3, 2, 0]
            for i, m in enumerate(riff):
                t.note(bar * 4 + i * 0.5, 0.45, k.degree(d + m, 3), "square", 0.045, duty=0.3)
    t.render("forge")


def boss():
    k = Key("E", "minor", 2)
    t = Track(150, 16)
    prog = [0, 5, 6, 4] * 4
    for bar, d in enumerate(prog):
        for s in range(8):
            t.note(bar * 4 + s * 0.5, 0.45, k.degree(d, 0), "square", 0.13, duty=0.4, cutoff=1600)
        for b in range(8):
            t.kick(bar * 4 + b * 0.5, 0.45)
        t.snare(bar * 4 + 1); t.snare(bar * 4 + 3)
        for s in range(16):
            t.note(bar * 4 + s * 0.25, 0.22, k.degree(d + [0, 2, 4, 7][s % 4], 2), "square", 0.045, duty=0.125)
        t.pad(bar * 4, 4, chords(k, [d], 1)[0], gain=0.06)
    t.render("boss")


def final():
    k = Key("B", "harmonic", 2)
    t = Track(140, 16)
    prog = [0, 5, 3, 4] * 4
    for bar, d in enumerate(prog):
        t.pad(bar * 4, 4, chords(k, [d], 1)[0] + [k.degree(d, 2)], gain=0.1, cutoff=1800)
        for s in range(8):
            t.note(bar * 4 + s * 0.5, 0.45, k.degree(d, -0), "saw", 0.1, cutoff=1000)
        t.kick(bar * 4); t.kick(bar * 4 + 0.75); t.kick(bar * 4 + 2); t.kick(bar * 4 + 2.75)
        t.snare(bar * 4 + 1); t.snare(bar * 4 + 3)
        t.hat(bar * 4 + 0.5); t.hat(bar * 4 + 1.5); t.hat(bar * 4 + 2.5); t.hat(bar * 4 + 3.5, open_=True)
        melody = [[4, 3, 2, 1], [0, 1, 2, 4], [5, 4, 3, 2], [1, 2, 3, 6]][bar % 4]
        for i, m in enumerate(melody):
            t.note(bar * 4 + i, 0.9, k.degree(m, 2), "square", 0.065, duty=0.5, vibrato=0.006)
    t.render("final")


def victory():
    k = Key("C", "lydian", 4)
    t = Track(132, 3, loop=False)
    for i, (d, b) in enumerate([(0, 0), (2, 0.5), (4, 1), (7, 1.5), (4, 2.5), (7, 3), (9, 4), (11, 6)]):
        t.note(b, 0.45 if i < 7 else 3.5, k.degree(d), "square", 0.12, duty=0.5, release=0.3 if i == 7 else 0.08)
    t.pad(6, 4, [k.degree(0, -1), k.degree(2, -1), k.degree(4, -1)], gain=0.12)
    t.kick(0); t.kick(6); t.snare(6, 0.3)
    t.render("victory")


def gameover():
    k = Key("A", "minor", 3)
    t = Track(80, 2, loop=False)
    for i, d in enumerate([4, 3, 2, 1, 0]):
        t.note(i * 0.75, 0.7 if i < 4 else 3, k.degree(d, 1), "triangle", 0.25, release=0.4 if i == 4 else 0.1)
    t.pad(3, 4, [k.degree(0), k.degree(2), k.degree(4)], gain=0.1, cutoff=600)
    t.render("gameover")


def ending():
    k = Key("C", "lydian", 3)
    t = Track(84, 12)
    prog = [0, 4, 5, 3] * 3
    for bar, d in enumerate(prog):
        t.pad(bar * 4, 4, chords(k, [d], 1)[0], gain=0.09, cutoff=1300)
        t.note(bar * 4, 3.9, k.degree(d, -1), "triangle", 0.22, release=0.3)
        for s in range(4):
            t.note(bar * 4 + s, 0.9, k.degree(d + [0, 2, 4, 2][s], 2), "sine", 0.06, release=0.5)
        melody = [4, 6, 5, 4, 2, 4, 5, 7, 6, 4, 2, 0][bar]
        t.note(bar * 4 + 0.5, 3, k.degree(melody, 3), "square", 0.04, duty=0.5, vibrato=0.005, release=0.4)
    t.render("ending")


if __name__ == "__main__":
    for f in (title, hub, catacombs, torture, cellars, forge, boss, final, victory, gameover, ending):
        f()
