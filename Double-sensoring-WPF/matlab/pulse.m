function [ meanPulse ] = pulse(colorList)
% Ber�knar och plottar medelpulsen �ver n�got tidsintervall.
% Tidsintervallet best�ms av indata till funktionen

% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samplesPerSecPulse = 30;
% Slut inst�llningar

%% Info om m�tdata
numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;
% Slut p� info om m�tdata

% G�r l�ngden p� vectorns indata udda
if (mod(numberOfSamplesPulse,2) == 0) % om colorList �r j�mn i l�ngd
    colorList = colorList(2:numberOfSamplesPulse);
    numberOfSamplesPulse = length(colorList);
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
degreeOfPolynomialPulse = samplesPerSecPulse - 1;
smoothColorList = sgolayfilt(colorList,degreeOfPolynomialPulse,numberOfSamplesPulse);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksPulse, peakLocationPulse]=findpeaks(smoothColorList);
numberOfPeaksPulse=length(peakLocationPulse);
% Slut filtrering

%% Pulsens medelv�rde �ver antal sekunder
bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
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
plot(peakLocationPulse, heightOfPeaksPulse, 'red o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
% Slut plot av filtrerad data

end


