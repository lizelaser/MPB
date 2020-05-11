
//Function 'deshabilitarBotones' according to pagination logic
function deshabilitarBotones(paginatorPayload, identifier) {

    const currentPage = paginatorPayload.PaginaActual
    const totalPages = paginatorPayload.TotalPaginas
    const totalItems = paginatorPayload.TotalRegistros
    const currentPageLength = paginatorPayload.Listado.length
    const chunkSize = paginatorPayload.RegistrosPorPagina


    const itemsUntilCurrentPage = (currentPage - 1) * chunkSize + currentPageLength
    const maxSkip = (totalPages - 1) * chunkSize

    if (totalItems <= chunkSize) {
        $(`#siguiente_${identifier}`).prop("disabled", true);
        $(`#ultimo_${identifier}`).prop("disabled", true);
        $(`#inicio_${identifier}`).prop("disabled", true);
        $(`#anterior_${identifier}`).prop("disabled", true);
    } else {
        if (currentPage > 1) {
            $(`#inicio_${identifier}`).prop("disabled", false);
            $(`#anterior_${identifier}`).prop("disabled", false);
        } else {
            $(`#inicio_${identifier}`).prop("disabled", true);
            $(`#anterior_${identifier}`).prop("disabled", true);
        }
        if (itemsUntilCurrentPage <= maxSkip) {
            $(`#siguiente_${identifier}`).prop("disabled", false);
            $(`#ultimo_${identifier}`).prop("disabled", false);
        } else {
            $(`#siguiente_${identifier}`).prop("disabled", true);
            $(`#ultimo_${identifier}`).prop("disabled", true);
        }
    }
}

// Function filltable for fill table from data of database

function fillTable(tableId, trArgs, ...args) {
    let items = []

    args.forEach((arg) => {
        if (typeof arg === 'object') {
            if (arg.isExtra) items.push(arg.value)
            else {
                let row = '<td style="padding:0;'

                // Style flag.
                if (Array.isArray(arg.styles)) {
                    arg.styles.forEach((style) => {
                        row += `${style}`
                    })
                }
                row += '"'

                // Attributes flag.
                if (Array.isArray(arg.attributes)) {
                    arg.attributes.forEach((attribute) => {
                        row += ` ${attribute}`
                    })
                }
                row += `>${arg.value}</td>`
                items.push(row)
            }
        } else items.push(`<td style="padding:0">${arg}</td>`)

    })

    const table = `<tr id="${trArgs.keyword}_${trArgs.id}">${items.join()}</tr>`

    $(tableId).prepend(table)
}