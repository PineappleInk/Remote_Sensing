function [ meanPulse ] = pulse(colorList)
% Beräknar och plottar medelpulsen över något tidsintervall.
% Tidsintervallet bestäms av indata till funktionen

% Inställningar; välj dina inställnignar för koden här
samplesPerSecPulse = 30;
% Slut inställningar

%% Info om mätdata
numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(numberOfSamplesPulse,2) == 0) % om colorList är jämn i längd
    colorList = colorList(2:numberOfSamplesPulse);
    numberOfSamplesPulse = length(colorList);
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
degreeOfPolynomialPulse = samplesPerSecPulse - 1;
smoothColorList = sgolayfilt(colorList,degreeOfPolynomialPulse,numberOfSamplesPulse);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksPulse, peakLocationPulse]=findpeaks(smoothColorList);
numberOfPeaksPulse=length(peakLocationPulse);
% Slut filtrering

%% Pulsens medelvärde över antal sekunder
bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
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
plot(peakLocationPulse, heightOfPeaksPulse, 'red o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
% Slut plot av filtrerad data

end


