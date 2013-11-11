function [belBP, belE, logZ] = bploopyUseTest(G, pot, localEv)
%a modification of bploopyTest to feed in an exterior CRF
% Will it work? Who knows!
% compile this to a .dll using the command:
% mcc -B csharedlib:libbploopyUse bploopyUse.m
% You will need to be sure you have added murphy's matlab folder to your
% matlab path.

% see how accurate loopy BP is on random graphs with binary states

%seed = 0;
%rand('state', seed);

nnodes = length(G);
nlabels = length(localEv{1});
%nstates = ones(1,nnodes);
for i = 1:nnodes
    nstates(i) = length(localEv{i});
end

  %[belExactCell] = brute_force_inf_mrf2(G, pot, nstates, localEv);
  %belExact = cell2num(belExactCell);
  
  %Print out the args for debugging
%   fprintf('********************\nInput to LoopyBP:\n  G:\n');
%   for i=1:nnodes
%       for j=1:nnodes
%           fprintf('%d\t', G(i,j));
%       end
%       fprintf('\n');
%   end
%   
%   fprintf('*********************\n  pot:\n');
%   for k=1:length(pot);
%       for i=1:nlabels
%           for j=1:nlabels
%               fprintf('%d\t', pot{k}(i,j));
%           end
%           fprintf('\n');
%       end
%       fprintf('---------------\n');
%   end
%   
%   fprintf('**********************\n  localEv:\n');
%   for i=1:nlabels
%     for k=1:length(localEv);
%           fprintf('%d\t', localEv{k}(i));
%     end
%     fprintf('\n');
%   end
%   fprintf('**********************\n');
  
  [E, Nedges] = assignEdgeNums(G);
  engine = bploopyEngine(E, nstates);
  [belBPCell, belECell, logZ] = bploopyInfer(engine, pot, localEv);
  belBP = cell2num(belBPCell);
  belE = cell2num(belECell);
  
  %figure(2); clf
  %plot(belExact(1,:), 'ro-')
  %hold on
  %plot(belBP(1,:), 'bx-')
  %drawnow
  
  %err = sum(abs(belExact(1,:)-belBP(1,:)))/nnodes;

%mean(err,2)

