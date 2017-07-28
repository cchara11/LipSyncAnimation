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
#ifndef WIN32
#include <pthread.h>
#else
#include <windows.h>
#endif

#define MAX_READ 100000

/* some compilers may complain about this not being defined */
extern char * strdup(const char *s);

void usage(char * name){
    fprintf(stderr, "tts_sync - a sample TTS program that uses the CereVoice Engine API with\n");
    fprintf(stderr, "a callback function.  Audio is played using the CereVoice Audio library.\n");
    fprintf(stderr, "The spoken text is printed synchronously using two different methods.\n\n");
    fprintf(stderr, "tts_sync loads a voice, then speaks text/XML from an input file, or from\n");
    fprintf(stderr, "stdin if the input file is not supplied.  Optionally the audio output can be\n");
    fprintf(stderr, "written to a wave file.\n\n");
    fprintf(stderr, "Usage: %s [Options] voice_file license_file [input_file]\n", name);
    fprintf(stderr, "Options:\n"); 
    fprintf(stderr, " -h\t\t  Display help\n");
    fprintf(stderr, " -o output_file\t  Output audio to file\n");
    exit(0);
}

/* This structure contains the data to display and timing information. */
struct w_data {
    double ostart; /* start time relative to the spurt */
    double start;  /* start time relative to the beginning of synthesis */
    double oend;   /* end time relative to the spurt */
    double end;    /* end time relative to the beginning of synthesis */
    char * name;   /* string to display */
    CPRC_sc_audio * buf; /* audio buffer it belongs to */
    struct w_data *next;
};

/* A user data structure is passed into the callback function by the user.
   It allows access to external user-configurable data.
 */

typedef struct user_data {
    CPRC_sc_player * player;
    /* Add other user-specific settings here */
    double total_time;
    long int sample_rate;
    struct w_data *words;
} user_data;

/* Add a word with timing information to the callback structure */

void add_word(struct w_data ** words, double ostart, double start, double oend, double end, const char * name, CPRC_sc_audio * buf ) {
    struct w_data * word;
    while (*words) words = &((*words)->next);
    word = calloc(1, sizeof(struct w_data));
    word->start = start;
    word->end = end;
    word->ostart = ostart;
    word->oend = oend;
    word->name = strdup(name);
    word->buf = buf;
    word->next = NULL;
    *words = word;
    fprintf(stdout, "Adding at times: %8.15g %8.15g %s\n", start, end, name);
    fflush(stdout);
}

/* Separate thread for printing synchronised information */
#ifndef WIN32
void * print_messages(void * userdata) {
#else
DWORD WINAPI print_messages(void * userdata) {
#endif
    user_data * data = (user_data *) userdata;
  double t, t2, ts;
  struct w_data * w, *cw = NULL, *cw2 = NULL, *tw;
  CPRC_sc_audio * buf;
  int cnt = 0;
  while(1) {
      if (CPRC_sc_audio_busy(data->player) && !CPRC_sc_audio_paused(data->player)) {
	  t = CPRC_sc_player_stream_duration(data->player) / (double)data->sample_rate;
	  t2 = CPRC_sc_player_stream_time(data->player);

	  /* First method: using total stream duration */
	  w = data->words;
	  while (w && w->end < t) w = w->next;
	  if (w && w != cw && w->start <= t) {
	      fprintf(stderr, "Method 1: %d %8.15g %8.15g %s\n", cnt, t, w->start, w->name);
	      fflush(stderr);
	      cw = w;
	  }

	  /* Second method: using timing relative to the spurt. Note that
	     the code could be much simpler, but is written like this to be 
	     consistant with the first method. */
	  w = data->words;
	  while (w && w->buf == NULL) w = w->next;
	  if (w) {
	      buf = w->buf;
	      ts = CPRC_sc_audio_start_time(w->buf);
	      t = (t2 - ts) / (double)data->sample_rate;
	      while (w && w->oend < t && w->buf == buf) w = w->next;
	      /* New spurt! */
	      if (w && w->buf != buf && CPRC_sc_audio_status(buf) == CPRC_SC_PLAYED) { 
		  tw = data->words;
		  buf = w->buf;
		  ts = CPRC_sc_audio_start_time(w->buf);
		  t = (t2 - ts) / (double)data->sample_rate;
		  /* A better cleaup should be made here... */
		  while (tw) { if (tw->buf != w->buf) tw->buf = NULL; else break; tw = tw->next; }
		  /* CPRC_sc_audio_delete(buf); */
		  buf = w->buf;
	      }
	      if (w && w != cw2 && w->ostart <= t) {
		  fprintf(stderr, "Method 2: %d %8.15g %8.15g %s\n", cnt, t, w->ostart, w->name);
		  fflush(stderr);
		  cw2 = w;
	      }
	  }
	  CPRC_sc_sleep_msecs(1);
	  cnt++;
	  if (cnt % 1200 == 0) {
	      CPRC_sc_audio_pauseon(data->player);
	      CPRC_sc_sleep_msecs(100);
	      CPRC_sc_audio_pauseoff(data->player);
	  }
      }
  }
  return NULL;
}

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
    if (data->player) {
        /* Use the CereVoice Audio API to play the audio that has been
            returned, the audio is converted to a buffer and cued for
            playback.
        */
        buf = CPRC_sc_audio_short(CPRC_abuf_wav_data(abuf),  CPRC_abuf_wav_sz(abuf));
        CPRC_sc_audio_cue(data->player, buf);
        printf("Current audio time: %g; dur: %ld, %8.15g\n", CPRC_sc_player_stream_time(data->player), CPRC_sc_player_samples_sent(data->player), CPRC_sc_player_stream_duration(data->player));
    }
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
                  add_word(&(data->words), start, start + data->total_time, end, end + data->total_time, name, buf);
        } else if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_MARK) {
            printf("INFO: marker: %.3f %.3f %s\n", start, end, name);
        } else if (CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_ERROR) {
            printf("ERROR: could not retrieve transcription at '%d'", i);
        }
        fflush(stdout);
    }
    data->total_time += CPRC_abuf_wav_sz(abuf) / (double) data->sample_rate; 
}

int main(int argc, char * argv[]){

    CPRCEN_engine * eng;
    CPRCEN_channel_handle hc;
    user_data data = {NULL, 0, 0, NULL};
#ifdef WIN32
    HANDLE thread1;
#else
    pthread_t thread1;
#endif
    char * voice_file  = NULL;
    char * license_file  = NULL;
    char * text_file = NULL;
    char * file_out = NULL;
    char text_buffer[MAX_READ];
    char * ret;
    const char * freqstr;
    FILE * text_fp;
    int arg, i, res, freq, iret1;

    /* Processing arguments */
    arg = 0;
    for (i = 1; i < argc; i++) {
        /* Options */
        if (strcmp(argv[i], "-h") == 0)
            usage(argv[0]);
        else if (strcmp(argv[i], "-o") == 0) {
            i++;
            if (i < argc)
                file_out = argv[i];
            else
                usage(argv[0]);
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
    if (arg < 2 || arg > 3)
      usage(argv[0]);

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

    /* Example of accessing voice information.  Sample rate is
       required for setting up audio playback. */
    fprintf(stderr, "INFO: voice name '%s'\n", CPRCEN_channel_get_voice_info(eng, hc, "VOICE_NAME"));
    freqstr = CPRCEN_channel_get_voice_info(eng, hc, "SAMPLE_RATE");
    fprintf(stderr, "INFO: voice sample rate is '%s'\n", freqstr);
    freq = atoi(freqstr);
    data.sample_rate = freq;

    /* Set file output or audio playback depending on the command line
     options. */
    if (file_out) {
      data.player = NULL;
      CPRCEN_engine_channel_to_file(eng, hc, file_out, CPRCEN_RIFF);
    } else {
        data.player = CPRC_sc_player_new(freq);
#ifdef WIN32
        thread1 = CreateThread(NULL,NULL, print_messages, (void*) &data, 0, NULL);
#else
        iret1 = pthread_create( &thread1, NULL, print_messages, (void *)&data);
        (void) iret1; /* stops some compilers giving warnings */
#endif
        /* data.cur_buf = NULL;
           data.prev_buf = NULL; */
    }
    res = CPRCEN_engine_set_callback(eng, hc, &data, channel_callback);
    if (res) fprintf(stderr, "INFO: callback initialised\n");

    /* Load the text to generate */
    if (text_file == NULL) {
        text_fp = stdin;
    } else {
        text_fp = fopen(text_file, "rb");
        if (!text_file) {
            fprintf(stderr, "ERROR: unable to open text file '%s', exiting.\n", text_file);
            exit(-1);
        }
    }

    /* Synthesise input line-by-line */
    while (!feof(text_fp)) {
      ret = fgets(text_buffer, MAX_READ, text_fp);
      if (!ret)
          break;
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
        fprintf(stderr, "Played %ld samples, total duration: %8.15g\n", CPRC_sc_player_samples_sent(data.player), CPRC_sc_player_stream_duration(data.player));
        /* Clean up the audio player */
        CPRC_sc_player_delete(data.player);
    }

    /* Clean up. The engine deletion function cleans up all loaded
       voices and open channels */
    CPRCEN_engine_delete(eng);
    fclose(text_fp);

    return 0;
}
