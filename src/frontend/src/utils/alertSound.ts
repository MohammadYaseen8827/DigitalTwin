// Sound Configuration
export interface SoundConfig {
    enabled: boolean
    volume: number
    soundFile?: string
}

export interface SoundSettings {
    info: SoundConfig
    warning: SoundConfig
    critical: SoundConfig
    error: SoundConfig
    success: SoundConfig
}

// Default sound settings
export const defaultSoundSettings: SoundSettings = {
    info: { enabled: true, volume: 0.5 },
    warning: { enabled: true, volume: 0.6 },
    critical: { enabled: true, volume: 0.8 },
    error: { enabled: true, volume: 0.7 },
    success: { enabled: true, volume: 0.5 }
}

// Severity levels for sound mapping
export type AlertSeverity = 'info' | 'warning' | 'critical' | 'error'

// Generate alert sound frequency based on severity
const alertFrequencies: Record<AlertSeverity, { frequency: number; duration: number; type: OscillatorType }> = {
    info: { frequency: 800, duration: 200, type: 'sine' },
    warning: { frequency: 600, duration: 300, type: 'triangle' },
    critical: { frequency: 1000, duration: 150, type: 'square' },
    error: { frequency: 400, duration: 400, type: 'sawtooth' }
}

// Pattern sounds (multiple beeps)
const alertPatterns: Record<AlertSeverity, number[]> = {
    info: [1],
    warning: [1, 1],
    critical: [1, 1, 1],
    error: [1, 0, 1, 0, 1]
}

class AlertSoundSystem {
    private audioContext: AudioContext | null = null
    private settings: SoundSettings = { ...defaultSoundSettings }

    constructor() {
        // Initialize audio context on first user interaction
        this.initAudioContext()
    }

    private async initAudioContext(): Promise<void> {
        try {
            this.audioContext = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)()
        } catch (error) {
            console.warn('Web Audio API not supported:', error)
        }
    }

    async ensureAudioContext(): Promise<AudioContext | null> {
        if (!this.audioContext) {
            await this.initAudioContext()
        }
        if (this.audioContext?.state === 'suspended') {
            await this.audioContext.resume()
        }
        return this.audioContext
    }

    updateSettings(settings: Partial<SoundSettings>): void {
        this.settings = { ...this.settings, ...settings }
    }

    getSettings(): SoundSettings {
        return { ...this.settings }
    }

    async playAlertSound(severity: AlertSeverity): Promise<void> {
        const config = this.settings[severity]
        if (!config?.enabled) return

        const ctx = await this.ensureAudioContext()
        if (!ctx) return

        const pattern = alertPatterns[severity]
        const alertConfig = alertFrequencies[severity]
        let currentTime = ctx.currentTime

        for (let i = 0; i < pattern.length; i++) {
            if (pattern[i] === 1) {
                this.playTone(
                    ctx,
                    alertConfig.frequency,
                    currentTime,
                    alertConfig.duration * 0.8,
                    config.volume,
                    alertConfig.type
                )
            }
            currentTime += alertConfig.duration
        }
    }

    private playTone(
        ctx: AudioContext,
        frequency: number,
        startTime: number,
        duration: number,
        volume: number,
        type: OscillatorType
    ): void {
        const oscillator = ctx.createOscillator()
        const gainNode = ctx.createGain()

        oscillator.type = type
        oscillator.frequency.setValueAtTime(frequency, startTime)

        // Smooth envelope
        gainNode.gain.setValueAtTime(0, startTime)
        gainNode.gain.linearRampToValueAtTime(volume, startTime + 0.01)
        gainNode.gain.setValueAtTime(volume, startTime + duration - 0.02)
        gainNode.gain.linearRampToValueAtTime(0, startTime + duration)

        oscillator.connect(gainNode)
        gainNode.connect(ctx.destination)

        oscillator.start(startTime)
        oscillator.stop(startTime + duration)

        // Cleanup
        setTimeout(() => {
            oscillator.disconnect()
            gainNode.disconnect()
        }, duration * 1000 + 100)
    }

    async playSuccessSound(): Promise<void> {
        const config = this.settings.success
        if (!config?.enabled) return

        const ctx = await this.ensureAudioContext()
        if (!ctx) return

        // Success chime - ascending tones
        const notes = [523.25, 659.25, 783.99] // C5, E5, G5
        const durations = [150, 150, 300]
        let currentTime = ctx.currentTime

        notes.forEach((note, index) => {
            this.playTone(ctx, note, currentTime, durations[index] * 0.8, config.volume, 'sine')
            currentTime += durations[index]
        })
    }

    async playNotificationSound(): Promise<void> {
        await this.playAlertSound('info')
    }
}

// Singleton instance
export const alertSound = new AlertSoundSystem()

// Reactive state holder for Vue composables
const soundState = {
    settings: { ...defaultSoundSettings }
}

export function useAlertSound() {
    function updateSettings(settings: Partial<SoundSettings>): void {
        alertSound.updateSettings(settings)
        soundState.settings = alertSound.getSettings()
    }

    async function playSound(severity: AlertSeverity): Promise<void> {
        await alertSound.playAlertSound(severity)
    }

    async function playSuccess(): Promise<void> {
        await alertSound.playSuccessSound()
    }

    return {
        settings: soundState.settings as SoundSettings,
        updateSettings,
        playSound,
        playSuccess
    }
}
