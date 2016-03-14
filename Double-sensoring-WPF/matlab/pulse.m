function [ meanPulse ] = pulse(colorList)
% Beräknar och plottar medelpulsen över något tidsintervall.
% Tidsintervallet bestäms av indata till funktionen

% Inställningar; välj dina inställnignar för koden här
samplesPerSec = 30;
% Slut inställningar

%% Info om mätdata
numberOfSamples = length(colorList);
timeOfMeasurement = numberOfSamples/samplesPerSec;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(numberOfSamples,2) == 0) % om colorList är jämn i längd
    colorList = colorList(2:numberOfSamples);
    numberOfSamples = length(colorList);
end
% Slut

%% Kontrollerar att listan är tillräckligt lång
if length(colorList)<4
    error('Not enough samples of pulse')
end
%Slut kontroll

%% Filtrera colorList med hjälp av högsta möjliga gradens
% Savitzky Golay FIR-filter
colorList=double(colorList); % Kanske ej behövs
degreeOfPolynomial = samplesPerSec - 1;
smoothColorList = sgolayfilt(colorList,degreeOfPolynomial,numberOfSamples);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaks, peakLocation]=findpeaks(smoothColorList);
numberOfPeaks=length(peakLocation);
% Slut filtrering

%% Pulsens medelvärde över antal sekunder
bpmPulse = (numberOfPeaks/timeOfMeasurement)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
meanPulse = bpmPulse
% Slut utskrifter

% Plot av filtrerad data
figure(1)
subplot(2,1,1)
hold off
plot(smoothColorList, 'red');
hold on
plot(peakLocation, heightOfPeaks, 'red o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
% Slut plot av filtrerad data

end


