(async function () {
    
    // CS: Well done moving these "magic" strings to their own variables.
    // .   I had hoped that someone would have called me out on that during
    // .   the live coding session. Alas, no one did. It is good to see you
    // .   thinking in this manner.
    const deckIdKey = 'DECK_ID';
    const computerPileName = 'computer_pile';
    const discardPileName = 'discard_pile';
    const humanPileName = 'human_pile';
    const computerDivId = 'computer';
    const playAreaDivId = 'play-area';
    const deckDivId = 'deck';
    const discardDivId = 'discard-pile';
    const humanDivId = 'human';
    const apiUrl = "https://deckofcardsapi.com/api/deck/";
    let computerPile = [];
    let discardPile = [];
    let humanPile = [];
    let remaining = 0;

    async function computerTurn() {
        const getPlayableCardIndex = function () {
            return computerPile.findIndex(card => matchCardCode(card.code, topCardCode));
        };
        const topCardCode = getTopCard().code;
        let cardIndex = getPlayableCardIndex();
        // debugger;
        while (cardIndex === -1) {
            const drawnCards = await drawDeckCards(1);
            await putCardsInPile(computerPileName, drawnCards);
            computerPile = await getPile(computerPileName);
            cardIndex = getPlayableCardIndex();
        }
        const playableCard = computerPile[cardIndex];
        await playCard(playableCard.code, computerPileName);
    }

    
    // CS: I just want to take a moment and discuss what I see with respect to the
    // .   functions that you've declared below. You have done really well at
    // .   putting like functions together to improve readability and maintenance
    // .   of your code base. When you go through this much effort, it hints at
    // .   a very source-code-oriented way of thinking.
    // . 
    // .   The next evolution of this code, in my opinion, would be to move the
    // .   state and the functions together into their own classes. All of the
    // .   deck-related stuff could go into a Deck class that allowed you to
    // .   reason holistically about the functionality, state, and persistence
    // .   of deck-related information.
    // . 
    // .   Just, overall, a really good job. Thank you for sharing this.
    
    
    /**
     * Returns an array of cards drawn from the deck.
     * @param {Number} num 
     */
    async function drawDeckCards(num) {
        const deckId = getDeckId();
        const response = await fetch(apiUrl + deckId + '/draw/?count=' + num);
        const obj = await response.json();
        const cards = obj.cards;
        remaining = obj.remaining;
        return cards;
    }

    /**
     * Draws and returns a card from a pile.
     * @param {string} pileName 
     * @param {string} code 
     */
    async function drawPileCard(pileName, code) {
        const deckId = getDeckId();
        const response = await fetch(apiUrl + deckId + '/pile/' + pileName + '/draw/?cards=' + code);
        const obj = await response.json();
        const card = obj.cards[0];
        return card;
    }

    /**
     * Returns a Promise for a deck.
     * @param {Number} id 
     */
    function fetchDeck(id) {
        return fetch(apiUrl + id + '/');
    }

    /**
     * Returns a Promise for a new shuffled deck.
     */
    function fetchNewShuffledDeck() {
        return fetch(apiUrl + '/new/shuffle/');
    }

    /**
     * Returns a Promise for the list of cards in a pile.
     * @param {string} pileName 
     */
    function fetchCardsInPile(pileName) {
        const deckId = getDeckId();
        return fetch(apiUrl + deckId + '/pile/' + pileName + '/list/');
    }

    /**
     * Returns the HTML element with the given ID.
     * @param {string} id 
     */
    function gebi(id) {
        return document.getElementById(id);
    }

    async function getDeck() {
        let deck;
        let response;
        // const deckId = getDeckId();
        // if (deckId !== null) {
        //     response = await fetchDeck(deckId);
        // } else {
            response = await fetchNewShuffledDeck();
        // }
        deck = await response.json();
        setDeckId(deck.deck_id);
        remaining = deck.remaining;
        return deck;
    }

    /**
     * Returns the deck ID in local storage.
     */
    function getDeckId() {
        return localStorage.getItem(deckIdKey);
    }

    /**
     * Returns the array of cards in a pile.
     * @param {string} pileName 
     */
    async function getPile(pileName) {
        const response = await fetchCardsInPile(pileName);
        const obj = await response.json();
        return obj.piles[pileName].cards;
    }

    /**
     * Returns the top card of the discard pile.
     */
    function getTopCard() {
        return discardPile[discardPile.length - 1];
    }

    async function initGame() {
        const deck = await getDeck();

        if (deck.remaining === 52) {
            // New deck; set up piles.
            const initialCards = await drawDeckCards(11);
            const topCardIndex = initialCards.findIndex(card => card.code[0] !== '8');
            const initDiscardPile = initialCards.splice(topCardIndex, 1);
            const initComputerPile = initialCards.slice(0, 5);
            const initHumanPile = initialCards.slice(5, 10);

            const putPileResponses = await Promise.all([
                putCardsInPile(computerPileName, initComputerPile),
                putCardsInPile(discardPileName, initDiscardPile),
                putCardsInPile(humanPileName, initHumanPile)
            ]);

            const putPileObjects = await Promise.all(putPileResponses.map(response => response.json()));
        }

        // Get piles
        const fetchPileResponses = await Promise.all([
            fetchCardsInPile(computerPileName),
            fetchCardsInPile(discardPileName),
            fetchCardsInPile(humanPileName)
        ]);
        const fetchPileObjects = await Promise.all(fetchPileResponses.map(response => response.json()));
        computerPile = fetchPileObjects[0].piles[computerPileName].cards;
        discardPile = fetchPileObjects[1].piles[discardPileName].cards;
        humanPile = fetchPileObjects[2].piles[humanPileName].cards;

        renderCards();
        registerEventHandlers();
    }

    /**
     * Returns whether or not two card codes match.
     * @param {string} playCardCode 
     * @param {string} topCardCode 
     */
    function matchCardCode(playCardCode, topCardCode) {
        return (playCardCode[0] === topCardCode[0])
            || (playCardCode[1] === topCardCode[1])
            || (playCardCode[0] === '8');
    }

    /**
     * Draws a card from the input pile and inserts it into the discard pile.
     * Afterwards, updates the local piles.
     * @param {string} cardCode 
     * @param {string} pileName 
     */
    async function playCard(cardCode, pileName) {
        const topCardCode = getTopCard().code;
        if (matchCardCode(cardCode, topCardCode)) {
            const playedCard = (await drawPileCard(pileName, cardCode));
            await putCardsInPile(discardPileName, [playedCard]);

            // Update local piles
            if (pileName === humanPileName) {
                humanPile = await getPile(pileName);
            } else {
                computerPile = await getPile(pileName);
            }
            discardPile = await getPile(discardPileName);
        }
    }
    async function putCardsInPile(pileName, cards) {
        const deckId = getDeckId();
        const cardCodes = cards.map(card => card.code).join(',');
        return await fetch(apiUrl + deckId + '/pile/' + pileName + '/add/?cards=' + cardCodes);
    }

    function registerEventHandlers() {
        gebi(humanDivId).addEventListener('click', async event => {
            const targetElement = event.target;
            if (!targetElement.hasAttribute('data-code')) {
                return;
            }
            const cardCode = targetElement.getAttribute('data-code');
            await playCard(cardCode, humanPileName);
            renderCards();
            // debugger;
            await computerTurn();
            renderCards();
        });
        gebi(deckDivId).addEventListener('click', async () => {
            const drawnCard = (await drawDeckCards(1))[0];
            await putCardsInPile(humanPileName, [drawnCard]);
            humanPile = await getPile(humanPileName);
            renderCards();
        });
    }

    function renderCards() {
        // computer cards
        const computerImages = computerPile
            .map(() => '<div class="card card-down"></div>');
        gebi(computerDivId).innerHTML = computerImages.join('');

        // human cards
        const humanImages = humanPile
            .map(card => '<img src="' + card.image + '" class="card card-up" data-code="' + card.code + '">');
        gebi(humanDivId).innerHTML = humanImages.join('');

        // play area
        const topCard = getTopCard();
        const topCardUrl = topCard.image;
        gebi(discardDivId).innerHTML = '<img src="'
            + topCardUrl + '" class="card card-up">';
    }

    function setDeckId(id) {
        localStorage.setItem(deckIdKey, id);
    }

    await initGame();
})();
