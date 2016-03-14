function [ meanBreath ] = breath( zList )
% Calculates mean number of breaths over some time-interval

% Inställningar; välj dina inställnignar för koden här
samplesPerSec = 30;
% Slut inställningar

%% Info om mätdata
numberOfSamples = length(zList);
timeOfMeasurement = numberOfSamples/30;
% Slut på info om mätdata

% Gör längden på vectorns indata udda
if (mod(numberOfSamples,2) == 0) % color_list är jämn i längd
    zList = zList(2:numberOfSamples);
    numberOfSamples = length(zList);
end
% Slut

%% Kontrollerar att listan är tillräckligt lång
if length(zList)<4
    error('Not enough samples of breath')
end
%Slut kontroll

%% Filtrera zList med hjälp av högsta möjliga gradens
% Savitzky Golay FIR-filter
zList=double(zList);

zList=double(zList); % Kanske ej behövs
degreeOfPolynomial = samplesPerSec - 1;
smoothZList = sgolayfilt(zList,degreeOfPolynomial,numberOfSamples);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaks, peakLocation]=findpeaks(smoothZList);
numberOfPeaks=length(peakLocation);
% Slut filtrering

%% Andningens medelvärde över antal sekunder
bpmBreath = (numberOfPeaks/timeOfMeasurement)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
meanBreath = bpmBreath
% Slut utskrifter

% Plot av filtrerad data
figure(1)
subplot(2,1,2)
hold off
plot(smoothZList, 'blue' );
hold on
plot(peakLocation, heightOfPeaks, 'blue o');
grid on
title({'Andetag per minut:', meanBreath}, 'color', 'blue', 'FontWeight', 'bold')
xlabel('tid [s/30]')
ylabel('Andningskurva')
% Slut plot

end


