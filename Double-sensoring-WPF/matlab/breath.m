function [ meanBreath ] = breath( zList )
% Calculates mean number of breaths over some time-interval

% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samplesPerSec = 30;
% Slut inst�llningar

%% Info om m�tdata
numberOfSamples = length(zList);
timeOfMeasurement = numberOfSamples/30;
% Slut p� info om m�tdata

% G�r l�ngden p� vectorns indata udda
if (mod(numberOfSamples,2) == 0) % color_list �r j�mn i l�ngd
    zList = zList(2:numberOfSamples);
    numberOfSamples = length(zList);
end
% Slut

%% Kontrollerar att listan �r tillr�ckligt l�ng
if length(zList)<4
    error('Not enough samples of breath')
end
%Slut kontroll

%% Filtrera zList med hj�lp av h�gsta m�jliga gradens
% Savitzky Golay FIR-filter
zList=double(zList);

zList=double(zList); % Kanske ej beh�vs
degreeOfPolynomial = samplesPerSec - 1;
smoothZList = sgolayfilt(zList,degreeOfPolynomial,numberOfSamples);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaks, peakLocation]=findpeaks(smoothZList);
numberOfPeaks=length(peakLocation);
% Slut filtrering

%% Andningens medelv�rde �ver antal sekunder
bpmBreath = (numberOfPeaks/timeOfMeasurement)*60;
% Slut medelv�rde �ver antal sekunder

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


