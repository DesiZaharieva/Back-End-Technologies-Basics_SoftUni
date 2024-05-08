function dvd_collection(dvds) {
    const numberOfDVDs = parseInt(dvds[0], 10);

    const allDVDs = dvds.slice(1, numberOfDVDs + 1);
    const allCommands = dvds.slice(numberOfDVDs + 1)

    for (let index = 0; index < allCommands.length; index += 1) {
        const rawParams = allCommands[index].split(' ');
        const commandName = rawParams[0];

        if (commandName === 'Watch') {
            const watchedDVD = allDVDs.shift();
            console.log(`${watchedDVD} DVD watched!`);
        } else if (commandName === 'Buy') {
            const dvdTitleToBuy = allCommands[index].slice(4);

            if (!dvdTitleToBuy) {
                continue;
            }
            allDVDs.push(dvdTitleToBuy);
        }else if (commandName === 'Swap') {
            const firstIndex = parseInt(rawParams[1], 10);
            const secondIndex = parseInt(rawParams[2], 10);

            if (isNaN(firstIndex) || firstIndex < 0 || firstIndex >= allDVDs.length) {
                continue;
            }

            if (isNaN(secondIndex) || secondIndex < 0 || secondIndex >= allDVDs.length) {
                continue;
            }

            const dvdOnFirstIndex = allDVDs[firstIndex];
            allDVDs[firstIndex] = allDVDs[secondIndex];
            allDVDs[secondIndex] = dvdOnFirstIndex;

            console.log("Swapped!");
        }else if (commandName === "Done") {
            break;
        }
    }

    if (allDVDs.length) {
        console.log(`DVDs left: ${allDVDs.join(', ')}`)
    } else {
        console.log("The collection is empty");
    }
}
 
dvd_collection (['3', 'The Matrix', 'The Godfather', 'The Shawshank Redemption', 'Watch', 'Done', 'Swap 0 1'])