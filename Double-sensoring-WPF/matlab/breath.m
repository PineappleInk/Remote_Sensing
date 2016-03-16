function [ meanBreath ] = breath( zList )
% Calculates mean number of breaths over some time-interval

% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samplesPerSecBreath = 30;
% Slut inst�llningar

%% Info om m�tdata
numberOfSamplesBreath = length(zList);
timeOfMeasurementBreath = numberOfSamplesBreath/30;
% Slut p� info om m�tdata

% G�r l�ngden p� vectorns indata udda
if (mod(numberOfSamplesBreath,2) == 0) % color_list �r j�mn i l�ngd
    zList = zList(2:numberOfSamplesBreath);
    numberOfSamplesBreath = length(zList);
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
degreeOfPolynomialBreath = samplesPerSecBreath - 1;
smoothZList = sgolayfilt(zList,degreeOfPolynomialBreath,numberOfSamplesBreath);
% V�nd p� kurvan f�r mer naturligt gr�nssnitt och ber�kning av
% inandnignar
smoothZList = smoothZList.*(-1);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksBreath, peakLocationBreath]=findpeaks(smoothZList);
numberOfPeaksBreath=length(peakLocationBreath);
% Slut filtrering

%% Andningens medelv�rde �ver antal sekunder
bpmBreath = (numberOfPeaksBreath/timeOfMeasurementBreath)*60;
% Slut medelv�rde �ver antal sekunder

% Utskrifter
meanBreath = bpmBreath
% Slut utskrifter

% Plot av filtrerad data
figure('visible', 'off')
subplot(2,1,2)
hold off
plot(smoothZList, 'blue' );
hold on
plot(peakLocationBreath, heightOfPeaksBreath, 'blue o');
grid on
title({'Andetag per minut:', meanBreath}, 'color', 'blue', 'FontWeight', 'bold')
xlabel('tid [s/30]')
ylabel('Andningskurva')
% Slut plot

end


