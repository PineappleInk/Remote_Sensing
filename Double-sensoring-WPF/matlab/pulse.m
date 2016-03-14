function [ meanPulse ] = pulse(colorList)
% Ber�knar och plottar medelpulsen �ver n�got tidsintervall.
% Tidsintervallet best�ms av indata till funktionen

% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samplesPerSec = 30;
% Slut inst�llningar

%% Info om m�tdata
numberOfSamples = length(colorList);
timeOfMeasurement = numberOfSamples/samplesPerSec;
% Slut p� info om m�tdata

% G�r l�ngden p� vectorns indata udda
if (mod(numberOfSamples,2) == 0) % om colorList �r j�mn i l�ngd
    colorList = colorList(2:numberOfSamples);
    numberOfSamples = length(colorList);
end
% Slut

%% Kontrollerar att listan �r tillr�ckligt l�ng
if length(colorList)<4
    error('Not enough samples of pulse')
end
%Slut kontroll

%% Filtrera colorList med hj�lp av h�gsta m�jliga gradens
% Savitzky Golay FIR-filter
colorList=double(colorList); % Kanske ej beh�vs
degreeOfPolynomial = samplesPerSec - 1;
smoothColorList = sgolayfilt(colorList,degreeOfPolynomial,numberOfSamples);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaks, peakLocation]=findpeaks(smoothColorList);
numberOfPeaks=length(peakLocation);
% Slut filtrering

%% Pulsens medelv�rde �ver antal sekunder
bpmPulse = (numberOfPeaks/timeOfMeasurement)*60;
% Slut medelv�rde �ver antal sekunder

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


