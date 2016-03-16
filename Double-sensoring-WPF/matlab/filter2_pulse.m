function [ filteredPulse2] = filter2_pulse( colorList)
% Filter #2

close all
%% Inställningar; välj dina inställnignar för koden här
samplesPerSecPulse = 30;
% Slut inställningar

%% Info om mätdata
numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;
% Slut på info om mätdata
%%
% Figur 1 - uppe till vänster
figure(1)
subplot(2,2,1)
plot(colorList)
title('Rådata')

% Figur 3 - nere till vänster
fs = 30;
[pxx,f] = periodogram(colorList,[],[],fs);
figure(1)
subplot(2,2,3)
plot(f,pxx)
%([0.6, 3.7])
title('Periodogram')
xlabel('Frequency (cycles/second)')
ylabel('Magnitude')
xlim([0, 5])

%% Filter #2
bpFilt = designfilt('bandpassiir','FilterOrder',20, ...
         'HalfPowerFrequency1',0.85,'HalfPowerFrequency2',3.60, ...
         'SampleRate',30);
fvtool(bpFilt)
%dataIn = randn(1000,1);
dataIn=colorList';
dataOut = filter(bpFilt,dataIn);
filtered_colorList = dataOut;

%% Plotta filtrering
% Figur 2, uppe till höger
figure(1)
subplot(2,2,2)
plot(filtered_colorList)
title('Filtrerad rådata med BP filter')
xlabel('tid [s]')
ylabel('Amplitud')
xlim([0, 300])

%% Plotta frekvenser av filtrerad data

% Figur 4 - nere till höger
% Frekvenser i filtrerad data
fs = 30;
[pxx,f] = periodogram(filtered_colorList,[],[],fs);
figure(1)
subplot(2,2,4)
plot(f,pxx)
%xlim([0.6, 3.7])
title('Periodogram av filtrerad signal')
xlabel('Frequency (cycles/second)')
ylabel('Magnitude')

%% Hitta peakar i filtrerad data
% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksPulse, peakLocationPulse]=findpeaks(filtered_colorList);
numberOfPeaksPulse = length(peakLocationPulse);
%pulse = numberOfPeaksPulse*60;
%% Pulsens medelvärde över antal sekunder
bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
meanPulse = round(bpmPulse)
% Slut utskrifter

%% Plotta peakarna

% Plot av filtrerad data
figure(2)
%subplot(2,1,1)
hold off
plot(filtered_colorList, 'red');
hold on
plot(peakLocationPulse, heightOfPeaksPulse, 'red o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
hold off
% Slut plot av filtrerad data

end


