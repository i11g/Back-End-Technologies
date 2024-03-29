function library(input = []) {

    let numberOfBooks = parseInt(input[0]);

    let books = input.slice(1, numberOfBooks + 1);

    let commands = input.slice(numberOfBooks + 1);

    for (let index = 0; index < commands.length; index++) {
        const listOfCommands = commands[index].split(" ");
        const commandToExecute = listOfCommands[0];

        if (commandToExecute === "Lend") {
            if (books.length > 0) {
                let booktoRemove = books.shift();
                console.log(`${booktoRemove} book lent!`)
            }
        }
        else if (commandToExecute === "Return") {
            const bookTitle = commands[index].slice(7);
            books.unshift(bookTitle);
        }
        else if (commandToExecute === "Exchange") {
            const startIndex = parseInt(listOfCommands[1]);
            const endIndex = parseInt(listOfCommands[2]);

            if (isNaN(startIndex) || startIndex < 0 || startIndex > books.length) {
                continue;
            }
            if (isNaN(endIndex) || endIndex < 0 || endIndex > books.length) {
                continue;
            }

            const tempIndex = books[startIndex];
            books[startIndex] = books[endIndex]
            books[endIndex] = tempIndex;
            console.log("Exchanged!");
        }

        else if (commandToExecute === "Stop") {
            break;
        }

    }
    if (books.length > 0) {
        console.log(`Books left: ${books.join(", ")}`);
    }
    else {
        console.log("The library is empty")
    }


}
library(['5', 'The Catcher in the Rye', 'To Kill a Mockingbird', 'The Great Gatsby', '1984', 'Animal Farm', 'Return Brave New World', 'Exchange 1 4', 'Stop'])