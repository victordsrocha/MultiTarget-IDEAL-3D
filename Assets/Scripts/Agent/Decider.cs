using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Decider : MonoBehaviour
{
    private Memory _memory;
    private Interaction _enactedInteraction;
    private Interaction _superInteraction;
    private Interaction _higherLevelSuperInteraction1;
    private Interaction _higherLevelSuperInteraction2;

    private void Start()
    {
        _memory = GetComponent<Memory>();
        _enactedInteraction = null;
        _superInteraction = null;
    }

    /// <summary>
    /// The current enacted interaction activates previously learned composite interactions as it matches
    /// their pre-interaction, then the agent gets the activated composited interaction set.
    /// </summary>
    List<Interaction> GetActivatedInteractions()
    {
        var contextInteractions = new List<Interaction>();
        if (_enactedInteraction != null)
        {
            contextInteractions.Add(_enactedInteraction);
            if (!_enactedInteraction.IsPrimitive())
            {
                contextInteractions.Add(_enactedInteraction.PostInteraction);
            }

            if (_superInteraction != null)
            {
                contextInteractions.Add(_superInteraction);
            }

            if (_higherLevelSuperInteraction1 != null)
            {
                contextInteractions.Add(_higherLevelSuperInteraction1);
            }

            if (_higherLevelSuperInteraction2 != null)
            {
                contextInteractions.Add(_higherLevelSuperInteraction2);
            }
        }

        var activatedInteractions = new List<Interaction>();
        foreach (var knowInteraction in _memory.KnownInteractions.Values)
        {
            if (knowInteraction.IsPrimitive()) continue;
            if (contextInteractions.Contains(knowInteraction.PreInteraction))
            {
                activatedInteractions.Add(knowInteraction);
            }
        }

        return activatedInteractions;
    }


    public List<Anticipation> GetDefaultAnticipations()
    {
        var anticipations = new List<Anticipation>();
        foreach (var experience in _memory.KnownExperiments.Values)
        {
            var defaultExperience = experience;
            if (defaultExperience.IntendedInteraction.is_primitive())
            {
                var anticipation = new Anticipation(experience, 0);
                anticipations.Add(anticipation);
            }
        }

        return anticipations;
    }

    public List<Anticipation> Anticipate()
    {
        List<Anticipation> anticipations = GetDefaultAnticipations();
        List<Interaction> activatedInteractions = GetActivatedInteractions();

        // este bloco cria uma lista de anticipations a partir da lista de interações ativadas
        if (_enactedInteraction != null)
        {
            foreach (var activatedInteraction in activatedInteractions)
            {
                if (activatedInteraction.PostInteraction.Experiment != null)
                {
                    var newAnticipation = new Anticipation
                    (
                        activatedInteraction.PostInteraction.Experiment,
                        activatedInteraction.Weight * activatedInteraction.PostInteraction.valence
                    );

                    var found = false;
                    foreach (var anticipation in anticipations)
                    {
                        if (newAnticipation.Experiment == anticipation.Experiment)
                        {
                            anticipation.AddProclivity(activatedInteraction.Weight *
                                                       activatedInteraction.PostInteraction.valence);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        anticipations.Add(newAnticipation);
                    }
                }
            }
        }

        /*
        * este bloco faz uso da lista de enacted interactions armazenadas em experiments
        * se uma dessas interactions é o postInteraction de uma interação ativada:
        * então podemos aumentar (ou diminuir?) a tendência (proclivity) da anticipation de origem
        */
        foreach (var anticipation in anticipations)
        {
            foreach (var experimentEnactedInteraction in anticipation.Experiment.EnactedInteractions)
            {
                foreach (var activatedInteraction in activatedInteractions)
                {
                    if (experimentEnactedInteraction == activatedInteraction.PostInteraction)
                    {
                        var proclivity = activatedInteraction.Weight * experimentEnactedInteraction.valence;
                        anticipation.AddProclivity(proclivity);
                    }
                }
            }
        }

        return anticipations;
    }

    private Experiment GetOtherExperiment(Experiment experiment)
    {
        /*
         * Acessa memória de experimentos conhecidos e retorna um experimento diferente do
         * recebido como argumento.
         * Por enquanto está sendo feito de forma aleatória.
         *
         * TODO refazer esta função usando Set:
         * experiments_set = set(self.memory.known_experiments.values()) - {experiment}
         * return random.choice(list(experiments_set))
         */
        var experiments = _memory.KnownExperiments.Values.ToList();
        var index = Random.Range(0, experiments.Count);
        var other = experiments[index];

        if (other == experiment)
        {
            other = GetOtherExperiment(experiment);
        }

        return other;
    }

    private Experiment GetOtherExperimentPrimitive()
    {
        var primitiveAnticipations = GetDefaultAnticipations();
        var randomPos = Random.Range(0, primitiveAnticipations.Count);
        return primitiveAnticipations[randomPos].Experiment;
        //return primitiveAnticipations[0].Experiment;
    }

    public Experiment SelectExperiment(List<Anticipation> anticipations)
    {
        /*
        The selectExperiment( ) function sorts the list of anticipations by decreasing proclivity of
        their proposed interaction. Then, it takes the fist anticipation (index [0]), which has
        the highest proclivity in the list. If this proclivity is positive, then the agent wants to
        re-enact this proposed interaction, leading to the agent choosing this proposed
        interaction's experiment.

        Por enquanto: se houver alguma anticipation com proclivity > 0 então a anticipation de
        maior proclivity sempre será escolhida, caso contrário sorteia um experimento conhecido
        qualquer para retornar.
        
        Acho que seria melhor tirar esse sorteio.... pois ignora o fato de que meu agente tem um
        impeto de tentar agir de acordo com o que gostou/ não gostou
        Assim sendo não faz sentido escolher aleatoriamente um -10 se há um -4

        Args:
            Anticipation list -> Lista de antecipações criadas por Anticipate( )

        Returns:
            Experiment -> experimento selecionado
        */
        Experiment selectedExperiment;
        if (anticipations.Count > 0)
        {
            anticipations = anticipations.OrderByDescending(x => x.Proclivity).ToList();

            /*
            foreach (var anticipation in anticipations)
            {
                Debug.Log("propose " + anticipation);
            }
            */


            var selectedAnticipation = anticipations[0];

            if (0.9 > Random.Range(0, 1))
            {
                // 90% escolhe a melhor anticipation independente se é maior que zero
                return selectedAnticipation.Experiment;
            }

            if (selectedAnticipation.Proclivity > 0f)
            {
                selectedExperiment = selectedAnticipation.Experiment;
            }
            else
            {
                // selectedExperiment = GetOtherExperiment(selectedAnticipation.Experiment);
                //selectedExperiment = selectedAnticipation.experiment;

                // não estou gostando de escolher aleatoriamente qualquer propose, mesmo que a proclivity seja muito
                // negativa

                // vou usar uma escolha aleatoria somente entre exp primitivos e depois pesquiso esse
                // problema

                // TODO

                selectedExperiment = GetOtherExperimentPrimitive();
            }
        }

        else
        {
            selectedExperiment = GetOtherExperimentPrimitive();
        }

        return selectedExperiment;
    }

    // essa função não deveria estar em memory?
    public void LearnCompositeInteraction(Interaction newEnactedInteraction)
    {
        var previousInteraction = _enactedInteraction;
        var lastInteraction = newEnactedInteraction;
        var previousSuperInteraction = _superInteraction;
        Interaction lastSuperInteraction = null;

        // learn [previous current] called the super interaction
        if (previousInteraction != null)
        {
            lastSuperInteraction =
                _memory.AddOrGetAndReinforceCompositeInteraction(previousInteraction, lastInteraction);
        }

        // Learn higher-level interactions
        if (previousSuperInteraction != null)
        {
            // learn [penultimate [previous current]]
            _higherLevelSuperInteraction1 = _memory.AddOrGetAndReinforceCompositeInteraction(
                previousSuperInteraction.PreInteraction,
                lastSuperInteraction);

            // learn [[penultimate previous] current]
            _higherLevelSuperInteraction2 =
                _memory.AddOrGetAndReinforceCompositeInteraction(previousSuperInteraction, lastInteraction);
        }

        this._superInteraction = lastSuperInteraction;

        // When a schema reaches a weight of 0 it is deleted from memory
        _memory.DecrementAndForgetSchemas(new List<Interaction>()
            {previousInteraction, lastInteraction, previousSuperInteraction, lastSuperInteraction});
    }
}