/* Copyright (c) 2010-2016 CereProc Ltd. */

/* Permission is hereby granted, free of charge, to any person obtaining */
/* a copy of this software and associated documentation files (the */
/* "Software"), to deal in the Software without restriction, including */
/* without limitation the rights to use, copy, modify, merge, publish, */
/* distribute, sublicense, and/or sell copies of the Software, and to */
/* permit persons to whom the Software is furnished to do so, subject to */
/* the following conditions: */

/* The above copyright notice and this permission notice shall be */
/* included in all copies or substantial portions of the Software. */

/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND */
/* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE */
/* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION */
/* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION */
/* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <cerevoice_eng.h>
#include <cerevoice_aud.h>

#define MAX_READ 100000

void usage(char * name){
    fprintf(stderr, "tts_callback - a sample TTS program that uses the CereVoice Engine API with\n");
    fprintf(stderr, "a callback function.  Audio is played using the CereVoice Audio library.\n\n");
    fprintf(stderr, "tts_callback loads a voice, then speaks text/XML from an input file, or from\n");
    fprintf(stderr, "stdin if the input file is not supplied.  Optionally the audio output can be\n");
    fprintf(stderr, "written to a wave file.\n\n");
    fprintf(stderr, "Usage: %s [Options] voice_file license_file [input_file]\n", name);
    fprintf(stderr, "Options:\n"); 
    fprintf(stderr, " -h\t\t  Display help\n");
    fprintf(stderr, " -o output_file\t  Output audio to file\n");
    fprintf(stderr, " -p <n>\t  Max number of phones in pipeline\n");
    exit(0);
}

/* A user data structure is passed into the callback function by the user.
   It allows access to external user-configurable data.
 */
typedef struct user_data {
    CPRC_sc_player * player;
    /* Add other user-specific settings here */
} user_data;

/* Callback function

   The callback function is fired for every phrase returned by the
   synthesiser.  This simple callback cues the audio buffer in the
   player object.

   To track the changes in status, we pass a pointer to a user data
   structure containing the audio player and the last audio buffer.
*/
void channel_callback(CPRC_abuf * abuf, void * userdata) {
    /* Transcription buffer, holds information on phone timings, 
       markers etc.
     */
    const CPRC_abuf_trans * trans;
    CPRC_sc_audio * buf = NULL;
    const char * name;
    float start, end;
    user_data * data = (user_data *) userdata;
    int i;
    int wav_mk;
    int wav_done;

    /* Used for processing when a min/max phone range has been set */
    wav_mk = CPRC_abuf_wav_mk(abuf);
    wav_done = CPRC_abuf_wav_done(abuf);
    printf("INFO: wav_mk %i, wav_done %i\n", wav_mk, wav_done);
    if (wav_mk < 0) wav_mk = 0;
    if (wav_done < 0) wav_done = 0;
    /* Process the transcription buffer items and print information. */
    for(i = 0; i < CPRC_abuf_trans_sz(abuf); i++) {
        trans = CPRC_abuf_get_trans(abuf, i);
        start = CPRC_abuf_trans_start(trans);
        end = CPRC_abuf_trans_end(trans);
        name = CPRC_abuf_trans_name(trans);
        if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_PHONE) {
            printf("INFO: phoneme: %.3f %.3f %s\n", start, end, name);
        } else if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_WORD) {
            printf("INFO: word: %.3f %.3f %s\n", start, end, name);
        } else if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_MARK) {
            printf("INFO: marker: %.3f %.3f %s\n", start, end, name);
        } else if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_ERROR) {
            printf("ERROR: could not retrieve transcription at '%d'", i);
        }
    }
    if (data->player) {
        /* Use the CereVoice Audio API to play the audio that has been
           returned, the audio is converted to a buffer and cued for
           playback.
         */
        buf = CPRC_sc_audio_short_disposable(CPRC_abuf_wav_data(abuf)+wav_mk, wav_done - wav_mk);
        CPRC_sc_audio_cue(data->player, buf);
    }
}

int main(int argc, char * argv[]){

    CPRCEN_engine * eng;
    CPRCEN_channel_handle hc;
    user_data data = {NULL};

    char * voice_file = NULL;
    char * license_file = NULL;
    char * text_file = NULL;
    char * file_out = NULL;
    char text_buffer[MAX_READ];
    char * ret;
    const char * freqstr;
    FILE * text_fp;
    int arg, i, res, freq, maxp = 0;
    
    /* Processing arguments */
    arg = 0;
    for (i = 1; i < argc; i++) {
        /* Options */
        if (strcmp(argv[i], "-h") == 0) usage(argv[0]);
        else if (strcmp(argv[i], "-o") == 0) {
            i++;
            if (i < argc) {
                file_out = argv[i];
            }
            else usage(argv[0]);
        }
        else if (strcmp(argv[i], "-p") == 0) {
            i++;
            if (i < argc) {
                maxp = strtol(argv[i], NULL, 10);
            }
            else usage(argv[0]);
        }
        /* Arguments */
        else {
            switch(arg) {
            case 0:
                voice_file = argv[i];
                break;
            case 1:
                license_file = argv[i];
                break;
            case 2:
                text_file = argv[i];
                break;
            default:
                fprintf(stderr, "ERROR: unable to process argument '%s'\n", argv[i]);
                usage(argv[0]);
            }
            arg++;
        }
    }
    if (arg < 2 || arg > 3) usage(argv[0]);

    /* Create a empty engine object.  The engine maintains the list of
       loaded voices and makes them available to synthesis channels. */
    eng = CPRCEN_engine_new();
    /* Load a voice into the engine */
    res = CPRCEN_engine_load_voice(eng, license_file, NULL, voice_file, CPRC_VOICE_LOAD);
    if (!res) {
        fprintf(stderr, "ERROR: unable to load voice file '%s', exiting.\n", voice_file);
        exit(-1);
    }

    /* Open a synthesis channel. */
    hc = CPRCEN_engine_open_default_channel(eng);

    if (maxp > 0) {
        CPRCEN_channel_set_phone_min_max(eng, hc, 0, maxp);
    }

    /* Example of accessing voice information.  Sample rate is
       required for setting up audio playback. */
    fprintf(stderr, "INFO: voice name '%s'\n", CPRCEN_channel_get_voice_info(eng, hc, "VOICE_NAME"));
    freqstr = CPRCEN_channel_get_voice_info(eng, hc, "SAMPLE_RATE");
    fprintf(stderr, "INFO: voice sample rate is '%s'\n", freqstr);
    freq = atoi(freqstr);

    /* Set file output or audio playback depending on the command line
     options. */
    if (file_out) {
        data.player = NULL;
        CPRCEN_engine_channel_to_file(eng, hc, file_out, CPRCEN_RIFF);
    } else {
        data.player = CPRC_sc_player_new(freq); 
        /* data.cur_buf = NULL;
          data.prev_buf = NULL; */
    }
    res = CPRCEN_engine_set_callback(eng, hc, &data, channel_callback);
    if (res) fprintf(stderr, "INFO: callback initialised\n");

    /* Load the text to generate */
    if (text_file == NULL)
        text_fp = stdin;
    else {
        text_fp = fopen(text_file, "rb");
        if (!text_file) {
            fprintf(stderr, "ERROR: unable to open text file '%s', exiting.\n", text_file);
            exit(-1);
        }
    }

    /* Synthesise input line-by-line */
    while (!feof(text_fp)) {
        ret = fgets(text_buffer, MAX_READ, text_fp);
        if (!ret) break;
        fprintf(stderr, "INFO: text read '%s'\n", text_buffer);
        /* Synthesise the text buffer - the final argument is 'flush'.
           Do not flush the buffer until all the input is sent.
         */
        CPRCEN_engine_channel_speak(eng, hc, text_buffer, strlen(text_buffer), 0);
    }
    /* Finished processing, flush the buffer with empty input */
    CPRCEN_engine_channel_speak(eng, hc, "", 0, 1);

    /* If we're playing audio, wait for completion before quitting */
    if (data.player) {
        while (CPRC_sc_audio_busy(data.player)) {
            CPRC_sc_sleep_msecs(50);
        }
        /* Clean up the audio player */
        CPRC_sc_player_delete(data.player);
    }

    /* Clean up. The engine deletion function cleans up all loaded
       voices and open channels */
    CPRCEN_engine_delete(eng);
    fclose(text_fp);

    return 0;
}
